using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Notifications.API.HealthChecks;

public class HealthResponse
{
    public string Status { get; set; }
    public string Service { get; set; }
    public double TotalDurationMs { get; set; }
    public List<HealthCheckItem> Checks { get; set; } = new List<HealthCheckItem>();
}

public class HealthCheckItem
{
    public string Name { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }
    public double DurationMs { get; set; }
}

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
        response.Service = "Notifications.API";
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
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
