using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MiniApi.Middleware;
using Serilog;

namespace MiniApi.Extensions
{
    public static class MiddlewareExtensions
    {
        public static void UseAppMiddleware(this WebApplication app)
        {
            // Sección 5.5: tiene que ir primero para que todo el request use el mismo id
            app.UseMiddleware<CorrelationIdMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (httpContext, _, ex) =>
                    (ex != null) ? Serilog.Events.LogEventLevel.Error :
                        (httpContext.Request.Path.StartsWithSegments("/health"))
                            ? Serilog.Events.LogEventLevel.Verbose : Serilog.Events.LogEventLevel.Information;
            });

            app.UseMiddleware<AuditMiddleware>();

            // Sección 5.4: /health ejecuta todos los checks
            HealthCheckOptions opcionesGeneral = new HealthCheckOptions();
            opcionesGeneral.ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse;
            app.MapHealthChecks("/health", opcionesGeneral);

            // Sección 5.4: /health/ready solo ejecuta los checks con la etiqueta "ready" (la base de datos)
            HealthCheckOptions opcionesReady = new HealthCheckOptions();
            opcionesReady.Predicate = EsCheckReady;
            opcionesReady.ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse;
            app.MapHealthChecks("/health/ready", opcionesReady);

            // Sección 5.4: /health/live no ejecuta checks: solo confirma que el proceso responde
            HealthCheckOptions opcionesLive = new HealthCheckOptions();
            opcionesLive.Predicate = NingunCheck;
            opcionesLive.ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse;
            app.MapHealthChecks("/health/live", opcionesLive);

            app.MapHealthChecksUI(setup => setup.UIPath = "/health-ui");

            app.UseExceptionHandler();
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
}