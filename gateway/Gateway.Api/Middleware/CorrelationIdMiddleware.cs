using Serilog.Context;

namespace Gateway.Api.Middleware;

/// <summary>
/// İstek takibi için CorrelationId middleware'i.
/// Her gelen isteğe benzersiz bir CorrelationId atar ve tüm loglara ekler.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // İstekten CorrelationId al veya yeni oluştur
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        // Response header'a CorrelationId ekle
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        // HttpContext.Items'a da ekle (diğer middleware'lerin kullanımı için)
        context.Items["CorrelationId"] = correlationId;

        // Serilog LogContext'e CorrelationId ekle - tüm loglar bu değeri içerecek
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogDebug("Request started with CorrelationId: {CorrelationId}", correlationId);
            
            await _next(context);
            
            _logger.LogDebug("Request completed with CorrelationId: {CorrelationId}", correlationId);
        }
    }
}

/// <summary>
/// CorrelationIdMiddleware için extension method
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}
