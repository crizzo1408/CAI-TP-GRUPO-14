using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Json;

namespace MiniApi.Extensions
{
    public static class LoggingExtensions
    {
        public const string ServiceName = "Orders.API";

        /// <summary>
        /// Sección 5.3: consola en formato legible y archivo en JSON estructurado.
        /// Cada log incluye Timestamp, Level, Service, Endpoint, CorrelationId y ErrorCode cuando aplica.
        /// Hosting.Diagnostics registra el inicio y el fin de cada request con su duración.
        /// </summary>
        public static void AddAppLogging(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
                .Enrich.FromLogContext()                       // CorrelationId y Endpoint (los agrega CorrelationIdMiddleware)
                .Enrich.WithProperty("Service", ServiceName)

                // Consola: formato legible
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")

                // Archivo logs/orders-AAAAMMDD.json: JSON estructurado, un evento por línea
                .WriteTo.File(
                    formatter: new JsonFormatter(renderMessage: true),
                    path: "logs/orders-.json",
                    rollingInterval: RollingInterval.Day)

                // ALUMNO: LOG DE AUDITORÍA: solo registra cuando se consume un endpoint, es una auditoría limpia.
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(le =>
                    {
                        // Solo aceptar logs que vengan del Middleware de Serilog
                        // Esto elimina automáticamente los duplicados de Microsoft.AspNetCore.Mvc
                        var isSerilogMiddleware = Matching.FromSource("Serilog.AspNetCore.RequestLoggingMiddleware")(le);
                        if (!isSerilogMiddleware) return false;

                        // Excluir rutas irrelevantes
                        if (le.Properties.TryGetValue("RequestPath", out var pathValue) &&
                            pathValue is ScalarValue scalar && scalar.Value is string path)
                        {
                            return !path.Contains("/health", StringComparison.OrdinalIgnoreCase) &&
                                   !path.Contains("/swagger", StringComparison.OrdinalIgnoreCase);
                        }

                        return true;
                    })
                    .WriteTo.File(
                        path: "logs/audit.log",
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {CorrelationId} | {RequestMethod} | {RequestPath} | {StatusCode}{NewLine}",
                        rollingInterval: RollingInterval.Day))
                .CreateLogger();

            builder.Host.UseSerilog();
        }
    }
}