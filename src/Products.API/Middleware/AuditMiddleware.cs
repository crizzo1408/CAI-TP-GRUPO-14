using System.Text;
using System.Text.Json;

namespace MiniApi.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(
        RequestDelegate next,
        ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var shouldAudit =
            ShouldAuditMethod(context.Request.Method);

        if (shouldAudit == false)
        {
            await _next(context);
            return;
        }

        context.Request.EnableBuffering();

        var requestBody =
            await ReadBodyAsync(context.Request.Body);

        context.Request.Body.Position = 0;

        var originalResponseBody = context.Response.Body;

        await using var responseBuffer = new MemoryStream();

        context.Response.Body = responseBuffer;

        try
        {
            await _next(context);

            responseBuffer.Position = 0;

            var responseBody =
                await ReadBodyAsync(responseBuffer);

            responseBuffer.Position = 0;

            await responseBuffer.CopyToAsync(
                originalResponseBody);

            var endpoint = context.Request.Path.Value;

            if (endpoint == null)
            {
                endpoint = string.Empty;
            }

            _logger.LogInformation(
                "AUDIT {HttpMethod} {Endpoint} {StatusCode} " +
                "{@RequestBody} {@ResponseBody}",
                context.Request.Method,
                endpoint,
                context.Response.StatusCode,
                TryParseJson(requestBody),
                TryParseJson(responseBody));
        }
        finally
        {
            context.Response.Body = originalResponseBody;
        }
    }

    private static bool ShouldAuditMethod(string method)
    {
        if (method.Equals(
            "POST",
            StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        else if (method.Equals(
            "PUT",
            StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        else if (method.Equals(
            "DELETE",
            StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static async Task<string> ReadBodyAsync(
        Stream body)
    {
        using var reader = new StreamReader(
            body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var content = await reader.ReadToEndAsync();

        return content;
    }

    private static object? TryParseJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        try
        {
            var parsedContent =
                JsonSerializer.Deserialize<object>(raw);

            return parsedContent;
        }
        catch (JsonException)
        {
            return raw;
        }
    }
}