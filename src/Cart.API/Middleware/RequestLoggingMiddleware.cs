using System.Diagnostics;

namespace Cart.API.Middleware;

/// <summary>
/// Loguea el inicio y el fin de cada request, con el status y la duración.
/// No loguea /health ni /swagger para no llenar los logs de ruido.
/// </summary>
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
            _logger.LogInformation("Fin request {Method} {Path} → {StatusCode} en {ElapsedMs} ms",
                method, path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}
