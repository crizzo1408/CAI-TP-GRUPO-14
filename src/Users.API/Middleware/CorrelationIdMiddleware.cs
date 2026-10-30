using Serilog.Context;

namespace Users.API.Middleware;

/// <summary>
/// Asigna un X-Correlation-Id a cada request:
/// - si el request ya trae el header (por ejemplo, porque lo llamó otro microservicio), lo reutiliza;
/// - si no, genera uno nuevo.
/// Lo devuelve en el header de la respuesta y lo agrega a todos los logs del request.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemKey = "CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var value)
                            && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : Guid.NewGuid().ToString();

        // Queda disponible para el resto del request (ej: para incluirlo en las respuestas de error)
        context.Items[ItemKey] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // Todo lo que se loguee dentro de este bloque lleva CorrelationId y Endpoint
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("Endpoint", $"{context.Request.Method} {context.Request.Path}"))
        {
            await next(context);
        }
    }
}
