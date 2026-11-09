using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MiniApi.Middleware;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Events;

namespace MiniApi.Extensions;

public static class MiddlewareExtensions
{
    public static void UseAppMiddleware(
        this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();

        app.UseSerilogRequestLogging(
            ConfigureRequestLogging);

        app.UseExceptionHandler();

        app.UseMiddleware<AuditMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapHealthChecks(
            "/health",
            CreateGeneralHealthOptions());

        app.MapHealthChecks(
            "/health/ready",
            CreateReadyHealthOptions());

        app.MapHealthChecks(
            "/health/live",
            CreateLiveHealthOptions());
    }

    private static void ConfigureRequestLogging(
        RequestLoggingOptions options)
    {
        options.GetLevel = GetRequestLogLevel;

        options.EnrichDiagnosticContext =
            EnrichRequestLog;
    }

    private static void EnrichRequestLog(
        IDiagnosticContext diagnosticContext,
        HttpContext context)
    {
        var endpoint = context.Request.Path.Value;

        if (endpoint == null)
        {
            endpoint = string.Empty;
        }

        diagnosticContext.Set(
            "Endpoint",
            endpoint);

        diagnosticContext.Set(
            "CorrelationId",
            context.TraceIdentifier);
    }

    private static LogEventLevel GetRequestLogLevel(
        HttpContext context,
        double elapsed,
        Exception? exception)
    {
        if (exception != null)
        {
            return LogEventLevel.Error;
        }

        if (context.Response.StatusCode >= 500)
        {
            return LogEventLevel.Error;
        }

        if (context.Request.Path.StartsWithSegments(
            "/health"))
        {
            return LogEventLevel.Verbose;
        }

        return LogEventLevel.Information;
    }

    private static HealthCheckOptions
        CreateGeneralHealthOptions()
    {
        var options = new HealthCheckOptions();

        options.ResponseWriter =
            UIResponseWriter.WriteHealthCheckUIResponse;

        return options;
    }

    private static HealthCheckOptions
        CreateReadyHealthOptions()
    {
        var options = new HealthCheckOptions();

        options.Predicate = IsDatabaseCheck;

        options.ResponseWriter =
            UIResponseWriter.WriteHealthCheckUIResponse;

        return options;
    }

    private static HealthCheckOptions
        CreateLiveHealthOptions()
    {
        var options = new HealthCheckOptions();

        options.Predicate = IsApiCheck;

        options.ResponseWriter =
            UIResponseWriter.WriteHealthCheckUIResponse;

        return options;
    }

    private static bool IsDatabaseCheck(
        HealthCheckRegistration registration)
    {
        if (registration.Tags.Contains("database"))
        {
            return true;
        }

        return false;
    }

    private static bool IsApiCheck(
        HealthCheckRegistration registration)
    {
        if (registration.Tags.Contains("api"))
        {
            return true;
        }

        return false;
    }
}