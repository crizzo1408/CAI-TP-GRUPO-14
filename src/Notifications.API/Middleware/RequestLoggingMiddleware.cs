using System.Diagnostics;

namespace Notifications.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string path = context.Request.Path.ToString();
        if (path.StartsWith("/health") || path.StartsWith("/swagger"))
        {
            await _next(context);
            return;
        }

        string method = context.Request.Method;
        _logger.LogInformation("Inicio request {Method} {Path}", method, path);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            int statusCode = context.Response.StatusCode;

            if (statusCode >= 500)
            {
                _logger.LogError(
                    "Fin request {Method} {Path} → {StatusCode} en {ElapsedMs} ms",
                    method, path, statusCode, stopwatch.ElapsedMilliseconds);
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning(
                    "Fin request {Method} {Path} → {StatusCode} en {ElapsedMs} ms",
                    method, path, statusCode, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation(
                    "Fin request {Method} {Path} → {StatusCode} en {ElapsedMs} ms",
                    method, path, statusCode, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
