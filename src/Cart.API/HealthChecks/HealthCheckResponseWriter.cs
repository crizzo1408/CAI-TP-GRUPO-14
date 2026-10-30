using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Cart.API.HealthChecks;

/// <summary>
/// Escribe la respuesta de los health checks en JSON:
/// { "status": "Healthy|Degraded|Unhealthy", "service": "...", "checks": [...] }
/// </summary>
public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        var response = new
        {
            status = report.Status.ToString(),
            service = "Cart.API",
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs = e.Value.Duration.TotalMilliseconds
            })
        };

        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
