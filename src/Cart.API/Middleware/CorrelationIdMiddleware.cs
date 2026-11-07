using Serilog.Context;

namespace Cart.API.Middleware;

/// <summary>
/// Asigna un X-Correlation-Id a cada request:
/// - si el request ya trae el header (por ejemplo, porque lo llamó otro microservicio), lo reutiliza;
/// - si no, genera uno nuevo.
/// Lo devuelve en el header de la respuesta y lo agrega a todos los logs del request.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemKey = "CorrelationId";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId = context.Request.Headers[HeaderName].ToString();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        // Queda disponible para el resto del request (ej: para incluirlo en las respuestas de error)
        context.Items[ItemKey] = correlationId;

        // El header se agrega justo antes de enviar la respuesta. Se hace así (y no ahora)
        // porque si hay un error, el manejo de excepciones limpia los headers.
        context.Response.OnStarting(AgregarHeader, context);

        string endpoint = context.Request.Method + " " + context.Request.Path;

        // Todo lo que se loguee dentro de este bloque lleva CorrelationId y Endpoint
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("Endpoint", endpoint))
        {
            await _next(context);
        }
    }

    private static Task AgregarHeader(object state)
    {
        var context = (HttpContext)state;
        context.Response.Headers[HeaderName] = (string)context.Items[ItemKey];
        return Task.CompletedTask;
    }
}
