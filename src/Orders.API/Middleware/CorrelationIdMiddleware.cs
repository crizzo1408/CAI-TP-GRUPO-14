using Serilog.Context;

namespace MiniApi.Middleware
{
    /// <summary>
    /// Sección 5.5: cada request tiene un X-Correlation-Id.
    /// Si viene en el request se reutiliza; si no, se genera uno nuevo.
    /// Se devuelve en la respuesta y se agrega a todos los logs del request.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private const string CorrelationHeader = "X-Correlation-Id";
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId = context.Request.Headers[CorrelationHeader].ToString();

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();

                // Se guarda en el request para que ErrorResponseFactory.GetCorrelationId
                // y las llamadas a Users y Products usen este mismo id.
                context.Request.Headers[CorrelationHeader] = correlationId;
            }

            // El header se agrega justo antes de enviar la respuesta,
            // así no se pierde si el ExceptionHandler limpia la respuesta.
            context.Response.OnStarting(AgregarHeader, context);

            // Sección 5.3: CorrelationId y Endpoint en todos los logs del request
            string endpoint = context.Request.Method + " " + context.Request.Path;
            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("Endpoint", endpoint))
            {
                await _next(context);
            }
        }

        private static Task AgregarHeader(object state)
        {
            HttpContext context = (HttpContext)state;
            context.Response.Headers[CorrelationHeader] = context.Request.Headers[CorrelationHeader].ToString();
            return Task.CompletedTask;
        }
    }
}