using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Notifications.API.HealthChecks;
using Notifications.API.Middleware;

namespace Notifications.API.Extensions;

public static class MiddlewareExtensions
{
    public static void UseAppMiddleware(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();

        // RequestLogging queda por fuera del handler: registra el status final (400/404/500),
        // pero las excepciones de negocio son convertidas por UseExceptionHandler.
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseExceptionHandler();

        app.UseSwagger();
        app.UseSwaggerUI();
    }

    public static void MapAppEndpoints(this WebApplication app)
    {
        app.MapControllers();

        var generalOptions = new HealthCheckOptions();
        generalOptions.ResponseWriter = HealthCheckResponseWriter.WriteAsync;
        app.MapHealthChecks("/health", generalOptions);

        var readyOptions = new HealthCheckOptions();
        readyOptions.Predicate = check => check.Tags.Contains("ready");
        readyOptions.ResponseWriter = HealthCheckResponseWriter.WriteAsync;
        app.MapHealthChecks("/health/ready", readyOptions);

        var liveOptions = new HealthCheckOptions();
        liveOptions.Predicate = check => false;
        liveOptions.ResponseWriter = HealthCheckResponseWriter.WriteAsync;
        app.MapHealthChecks("/health/live", liveOptions);
    }
}
