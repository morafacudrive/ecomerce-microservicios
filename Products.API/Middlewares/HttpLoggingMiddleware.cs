using System.Diagnostics;
using Serilog;

namespace Products.API.Middlewares;
/// <summary>
/// Middleware que registra todas las peticiones HTTP con timing, status code y Correlation ID.
/// </summary>

public class HttpLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpLoggingMiddleware> _logger;

    public HttpLoggingMiddleware(RequestDelegate next, ILogger<HttpLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            var logLevel = statusCode >= 500 ? LogLevel.Error :
                          statusCode >= 400 ? LogLevel.Warning :
                          LogLevel.Information;

            _logger.Log(logLevel,
                "HTTP {Method} {Path} completó con {StatusCode} en {DurationMs}ms. CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path.Value ?? "/",
                statusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var correlationIdError = context.Items["CorrelationId"]?.ToString() ?? "N/A";
            
            _logger.LogError(ex,
                "HTTP {Method} {Path} falló en {DurationMs}ms. CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path.Value ?? "/",
                stopwatch.ElapsedMilliseconds,
                correlationIdError);

            throw;
        }
    }
}
