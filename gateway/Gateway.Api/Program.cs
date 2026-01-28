using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;

//Yapılandırılmış loglama için Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProperty("ServiceName", "Gateway.Api")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Gateway.Api (API Gateway)...");

    var builder = WebApplication.CreateBuilder(args);

   // Logs - Serilog entegrasyonu
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .MinimumLevel.Override("Yarp", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithMachineName()
        .Enrich.WithProperty("ServiceName", "Gateway.Api")
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] {Message:lj}{NewLine}{Exception}"));

    //Environment variables desteği
    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

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
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Gateway is healthy"), tags: new[] { "live" })
        .AddUrlGroup(new Uri("http://localhost:5101/health/live"), name: "identity-api", tags: new[] { "ready", "services" })
        .AddUrlGroup(new Uri("http://localhost:5102/health/live"), name: "products-api", failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded, tags: new[] { "ready", "services" });

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

    // Logs - HTTP request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseHttpsRedirection();

    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    // Health Checks endpoint'leri
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
    });

    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live")
    });

    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });

    // Map YARP reverse proxy
    app.MapReverseProxy();

    // Disposability - Graceful shutdown
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() => Log.Information("Gateway.Api started - routing requests to microservices"));
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
