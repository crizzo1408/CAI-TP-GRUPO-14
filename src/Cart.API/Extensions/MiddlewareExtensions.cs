using Cart.API.HealthChecks;
using Cart.API.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

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
        app.UseSwaggerUI(options => options.DocumentTitle = "Cart.API - Swagger");
    }

    public static void MapAppEndpoints(this WebApplication app)
    {
        app.MapControllers();

        // /health: todos los checks
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteAsync
        });

        // /health/ready: ¿está listo para atender? (verifica la base y Products.API)
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = HealthCheckResponseWriter.WriteAsync
        });

        // /health/live: ¿el proceso está vivo? (no ejecuta checks)
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = HealthCheckResponseWriter.WriteAsync
        });
    }
}
