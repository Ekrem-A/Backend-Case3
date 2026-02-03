using System.Text;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Products.Application.Behaviors;
using Products.Application.Commands;
using Products.Application.Interfaces;
using Products.Domain.Interfaces;
using Products.Infrastructure.Caching;
using Products.Infrastructure.Data;
using Products.Infrastructure.Messaging;
using Products.Infrastructure.Repositories;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;

// Seq URL - Environment variable
var seqUrl = Environment.GetEnvironmentVariable("SEQ_URL");

// Serilog yapılandırması
var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("ServiceName", "Products.Api")
    .Enrich.WithProperty("ServiceVersion", "1.0.0")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}");

if (!string.IsNullOrWhiteSpace(seqUrl) && Uri.TryCreate(seqUrl, UriKind.Absolute, out _))
{
    loggerConfig.WriteTo.Seq(seqUrl);
    Console.WriteLine($"[Products.Api] Seq logging enabled: {seqUrl}");
}
else
{
    Console.WriteLine($"[Products.Api] Seq logging disabled. SEQ_URL: '{seqUrl ?? "null"}'");
}

Log.Logger = loggerConfig.CreateBootstrapLogger();

try
{
    Log.Information("Starting Products.Api service...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog Host entegrasyonu
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("ServiceName", "Products.Api")
            .Enrich.WithProperty("ServiceVersion", "1.0.0")
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}");

        if (!string.IsNullOrWhiteSpace(seqUrl) && Uri.TryCreate(seqUrl, UriKind.Absolute, out _))
        {
            configuration.WriteTo.Seq(seqUrl);
        }
    });

    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    // DbContext
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ProductsDbContext>(options =>
        options.UseSqlServer(connectionString));

    // Redis Cache - hata toleranslı
    var redisEnabled = builder.Configuration.GetValue<bool>("Redis:Enabled");
    var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");

    if (redisEnabled && !string.IsNullOrEmpty(redisConnectionString))
    {
        try
        {
            Log.Information("Redis cache enabled, connecting to: {RedisConnection}", redisConnectionString);
            
            var redisConnection = ConnectionMultiplexer.Connect(redisConnectionString + ",abortConnect=false,connectTimeout=5000");
            builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);

            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "Products:";
            });

            builder.Services.AddSingleton<ICacheService, RedisCacheService>();

            builder.Services.AddDataProtection()
                .SetApplicationName("Backend-Services")
                .PersistKeysToStackExchangeRedis(redisConnection, "DataProtection-Keys");
            
            Log.Information("Redis cache and Data Protection configured successfully");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Redis connection failed, using NullCacheService");
            builder.Services.AddSingleton<ICacheService, NullCacheService>();
        }
    }
    else
    {
        Log.Information("Redis cache disabled, using NullCacheService");
        builder.Services.AddSingleton<ICacheService, NullCacheService>();
    }

    // Health Checks
    var healthChecksBuilder = builder.Services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: new[] { "live" });

    if (!string.IsNullOrEmpty(connectionString))
    {
        healthChecksBuilder.AddSqlServer(connectionString, tags: new[] { "db", "ready" });
    }

    // MediatR
    builder.Services.AddMediatR(cfg => {
        cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly);
    });

    // FluentValidation
    builder.Services.AddValidatorsFromAssembly(typeof(CreateProductCommand).Assembly);
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

    // Repositories
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

    // Kafka Event Publisher
    var kafkaEnabled = builder.Configuration.GetValue<bool>("Kafka:Enabled");
    if (kafkaEnabled)
    {
        builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));
        builder.Services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
    }
    else
    {
        builder.Services.AddSingleton<IEventPublisher, NullEventPublisher>();
    }

    // JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"]!;

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Products API",
            Version = "v1",
            Description = "Product Management API - CQRS Pattern"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // Seed database
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            await DbInitializer.SeedAsync(scope.ServiceProvider);
            Log.Information("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while seeding the database");
        }
    }

    // Swagger her zaman açık (Development ve Production)
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Products API v1");
        c.RoutePrefix = "swagger";
    });

    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();

    // Health Checks
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    duration = e.Value.Duration.TotalMilliseconds
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            });
            await context.Response.WriteAsync(result);
        }
    });

    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live")
    });

    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready") || check.Tags.Contains("db")
    });

    app.MapControllers();

    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() => Log.Information("Products.Api started successfully"));
    lifetime.ApplicationStopping.Register(() => Log.Information("Products.Api is shutting down..."));

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Products.Api terminated unexpectedly");
}
finally
{
    Log.Information("Products.Api shutdown complete");
    await Log.CloseAndFlushAsync();
}
