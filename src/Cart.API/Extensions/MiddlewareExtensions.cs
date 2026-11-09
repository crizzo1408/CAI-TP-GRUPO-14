using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Cart.API.HealthChecks;
using Cart.API.Middleware;

namespace Cart.API.Extensions;

public static class MiddlewareExtensions
{
    /// <summary>
    /// Orden de los middlewares (importa):
    /// 1. Correlation ID: primero, para que todo lo demás ya lo tenga en los logs.
    /// 2. Log de inicio/fin del request.
    /// 3. UseExceptionHandler: convierte las excepciones en respuestas de error (IExceptionHandler).
    /// 4. Swagger UI en /swagger.
    /// </summary>
    public static void UseAppMiddleware(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseExceptionHandler();

        app.UseSwagger();
        app.UseSwaggerUI();
    }

    public static void MapAppEndpoints(this WebApplication app)
    {
        app.MapControllers();

        // /health: todos los checks
        var opcionesGeneral = new HealthCheckOptions();
        opcionesGeneral.ResponseWriter = HealthCheckResponseWriter.WriteAsync;
        app.MapHealthChecks("/health", opcionesGeneral);

        // /health/ready: ¿está listo para atender? (la base y Products.API: los checks con la etiqueta "ready")
        var opcionesReady = new HealthCheckOptions();
        opcionesReady.Predicate = EsCheckReady;
        opcionesReady.ResponseWriter = HealthCheckResponseWriter.WriteAsync;
        app.MapHealthChecks("/health/ready", opcionesReady);

        // /health/live: ¿el proceso está vivo? (no ejecuta ningún check)
        var opcionesLive = new HealthCheckOptions();
        opcionesLive.Predicate = NingunCheck;
        opcionesLive.ResponseWriter = HealthCheckResponseWriter.WriteAsync;
        app.MapHealthChecks("/health/live", opcionesLive);
    }

    private static bool EsCheckReady(HealthCheckRegistration check)
    {
        return check.Tags.Contains("ready");
    }

    private static bool NingunCheck(HealthCheckRegistration check)
    {
        return false;
    }
}
