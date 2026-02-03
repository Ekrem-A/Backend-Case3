using System.Text;
using System.Threading.RateLimiting;
using Gateway.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;

// Seq URL - Environment variable veya default
var seqUrl = Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341";

//Yapılandırılmış loglama için Serilog (Seq entegrasyonu ile)
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("ServiceName", "Gateway.Api")
    .Enrich.WithProperty("ServiceVersion", "1.0.0")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Seq(seqUrl)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Gateway.Api (API Gateway)...");

    var builder = WebApplication.CreateBuilder(args);

   // Logs - Serilog entegrasyonu (Seq merkezi log toplama ile)
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Yarp", LogEventLevel.Information)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithMachineName()
        .Enrich.WithProperty("ServiceName", "Gateway.Api")
        .Enrich.WithProperty("ServiceVersion", "1.0.0")
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.Seq(seqUrl));

    //Environment variables desteği
    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    // Redis Data Protection - Container restart'larında key'lerin korunması için
    var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");
    if (!string.IsNullOrEmpty(redisConnectionString))
    {
        var redis = ConnectionMultiplexer.Connect(redisConnectionString);
        builder.Services.AddDataProtection()
            .SetApplicationName("Backend-Services")
            .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
        
        Log.Information("Data Protection configured with Redis persistence");
    }

    // Rate Limiting Configuration
    builder.Services.AddRateLimiter(options =>
    {
        // Global rate limit rejection response
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.ContentType = "application/json";
            
            var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                ? retryAfterValue.TotalSeconds
                : 60;

            context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

            Log.Warning("Rate limit exceeded for {ClientIP} on {Path}",
                context.HttpContext.Connection.RemoteIpAddress,
                context.HttpContext.Request.Path);

            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Too Many Requests",
                message = "Rate limit exceeded. Please try again later.",
                retryAfterSeconds = retryAfter
            }, cancellationToken);
        };

        // Fixed Window - Genel API istekleri için (100 istek / dakika)
        options.AddFixedWindowLimiter("fixed", opt =>
        {
            opt.PermitLimit = 100;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 10;
        });

        // Sliding Window - Auth endpoint'leri için (20 istek / dakika)
        options.AddSlidingWindowLimiter("auth", opt =>
        {
            opt.PermitLimit = 20;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.SegmentsPerWindow = 4;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 5;
        });

        // Token Bucket - Yoğun okuma işlemleri için (burst destekli)
        options.AddTokenBucketLimiter("products", opt =>
        {
            opt.TokenLimit = 50;
            opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
            opt.TokensPerPeriod = 10;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 10;
            opt.AutoReplenishment = true;
        });

        // Concurrency Limiter - Admin işlemleri için (eşzamanlı 5 istek)
        options.AddConcurrencyLimiter("admin", opt =>
        {
            opt.PermitLimit = 5;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 3;
        });

        // IP bazlı global limiter
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 200,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 20
            });
        });
    });

    // Configure JWT Authentication
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

    // Add Authorization Policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("authenticated", policy =>
        {
            policy.RequireAuthenticatedUser();
        });

        options.AddPolicy("admin", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Admin");
        });
    });

    // Add YARP Reverse Proxy
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    // Health Checks - Gateway sağlık kontrolü
    var healthChecksBuilder = builder.Services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Gateway is healthy"), tags: new[] { "live" });

    if (!string.IsNullOrEmpty(redisConnectionString))
    {
        healthChecksBuilder.AddRedis(redisConnectionString, name: "redis", tags: new[] { "cache" });
    }

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // CorrelationId middleware - tüm isteklere benzersiz ID atar
    app.UseCorrelationId();

    // Logs - HTTP request logging (CorrelationId ile zenginleştirilmiş)
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("CorrelationId", httpContext.Items["CorrelationId"]?.ToString() ?? "N/A");
            diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A");
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        };
    });

    app.UseHttpsRedirection();

    app.UseCors("AllowAll");

    // Rate Limiting Middleware - CORS'tan sonra, Auth'tan önce
    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    // Health Checks endpoint'leri (rate limit dışında)
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                gateway = "YARP API Gateway",
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            });
            await context.Response.WriteAsync(result);
        }
    }).DisableRateLimiting();

    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live")
    }).DisableRateLimiting();

    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    }).DisableRateLimiting();

    // Map YARP reverse proxy with rate limiting
    app.MapReverseProxy(proxyPipeline =>
    {
        proxyPipeline.Use(async (context, next) =>
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            
            // Endpoint'e göre rate limit policy seç
            string? rateLimitPolicy = path switch
            {
                var p when p.Contains("/api/auth/") => "auth",
                var p when p.Contains("/api/admin/") => "admin",
                var p when p.Contains("/api/products") => "products",
                var p when p.Contains("/api/categories") => "products",
                _ => "fixed"
            };

            // Rate limit header ekle (debugging için)
            context.Response.Headers["X-RateLimit-Policy"] = rateLimitPolicy;
            
            await next();
        });
    });

    // Disposability - Graceful shutdown
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() => Log.Information("Gateway.Api started - routing requests to microservices with rate limiting enabled"));
    lifetime.ApplicationStopping.Register(() => Log.Information("Gateway.Api is shutting down..."));
    lifetime.ApplicationStopped.Register(() => Log.Information("Gateway.Api has stopped"));

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway.Api terminated unexpectedly");
}
finally
{
    // Disposability - Kaynakları temizle
    Log.Information("Gateway.Api shutdown complete");
    await Log.CloseAndFlushAsync();
}
