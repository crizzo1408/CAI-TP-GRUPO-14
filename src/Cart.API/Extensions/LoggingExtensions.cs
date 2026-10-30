using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Cart.API.Extensions;

public static class LoggingExtensions
{
    public const string ServiceName = "Cart.API";

    /// <summary>
    /// Serilog con dos salidas (sinks):
    /// - Consola: formato legible.
    /// - Archivo logs/cart-AAAAMMDD.json: JSON estructurado, un evento por línea.
    /// Cada log incluye Timestamp, Level, Service, Endpoint, CorrelationId y ErrorCode cuando aplica.
    /// </summary>
    public static void AddAppLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()                       // CorrelationId y Endpoint (los agrega el middleware)
            .Enrich.WithProperty("Service", ServiceName)
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                formatter: new JsonFormatter(renderMessage: true),
                path: "logs/cart-.json",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}
