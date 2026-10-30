using System.Diagnostics;

namespace Cart.API.Middleware;

/// <summary>
/// Loguea el inicio y el fin de cada request, con el status y la duración.
/// No loguea /health ni /swagger para no llenar los logs de ruido.
/// </summary>
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        if (path.StartsWithSegments("/health") || path.StartsWithSegments("/swagger"))
        {
            await next(context);
            return;
        }

        var method = context.Request.Method;
        logger.LogInformation("Inicio request {Method} {Path}", method, path.Value);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            logger.LogInformation("Fin request {Method} {Path} → {StatusCode} en {ElapsedMs} ms",
                method, path.Value, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}
