using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace MiniApi.Extensions;

public static class LoggingExtensions
{
    public static void AddAppLogging(
        this WebApplicationBuilder builder)
    {
        var jsonFormatter =
            new JsonFormatter(renderMessage: true);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override(
                "Microsoft",
                LogEventLevel.Warning)
            .MinimumLevel.Override(
                "Microsoft.AspNetCore.Hosting.Diagnostics",
                LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty(
                "Service",
                "Products.API")
            .WriteTo.Console(
                outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] " +
                "[{Service}] " +
                "{Message:lj}" +
                "{NewLine}{Exception}")
            .WriteTo.File(
                formatter: jsonFormatter,
                path: "logs/products-.json",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();

        builder.Host.UseSerilog();
    }
}