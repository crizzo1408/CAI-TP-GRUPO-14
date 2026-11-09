using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Notifications.API.Extensions;

public static class LoggingExtensions
{
    public const string ServiceName = "Notifications.API";

    public static void AddAppLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", ServiceName)
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] [{Service}] [{CorrelationId}] [{Endpoint}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                formatter: new JsonFormatter(renderMessage: true),
                path: "logs/notifications-.json",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}
