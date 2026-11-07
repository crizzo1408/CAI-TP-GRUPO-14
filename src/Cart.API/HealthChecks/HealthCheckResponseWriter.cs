using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Cart.API.HealthChecks;

/// <summary>Respuesta JSON de los health checks.</summary>
public class HealthResponse
{
    public string Status { get; set; }
    public string Service { get; set; }
    public double TotalDurationMs { get; set; }
    public List<HealthCheckItem> Checks { get; set; } = new List<HealthCheckItem>();
}

/// <summary>Resultado de un check individual (ej: sqlite).</summary>
public class HealthCheckItem
{
    public string Name { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }
    public double DurationMs { get; set; }
}

/// <summary>
/// Escribe la respuesta de los health checks en JSON:
/// { "status": "Healthy|Degraded|Unhealthy", "service": "...", "checks": [...] }
/// </summary>
public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        var response = new HealthResponse();
        response.Status = report.Status.ToString();
        response.Service = "Cart.API";
        response.TotalDurationMs = report.TotalDuration.TotalMilliseconds;

        foreach (var entry in report.Entries)
        {
            var item = new HealthCheckItem();
            item.Name = entry.Key;
            item.Status = entry.Value.Status.ToString();
            item.Description = entry.Value.Description;
            item.DurationMs = entry.Value.Duration.TotalMilliseconds;
            response.Checks.Add(item);
        }

        context.Response.ContentType = "application/json";
        string json = JsonSerializer.Serialize(response, JsonOptions);
        return context.Response.WriteAsync(json);
    }
}
