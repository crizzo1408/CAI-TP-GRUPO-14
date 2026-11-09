using Serilog.Context;

namespace Notifications.API.Middleware;

public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemKey = "CorrelationId";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId = context.Request.Headers[HeaderName].ToString();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Items[ItemKey] = correlationId;
        context.TraceIdentifier = correlationId;
        context.Response.OnStarting(AddHeader, context);

        string endpoint = context.Request.Method + " " + context.Request.Path;
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("Endpoint", endpoint))
        {
            await _next(context);
        }
    }

    private static Task AddHeader(object state)
    {
        var context = (HttpContext)state;
        context.Response.Headers[HeaderName] = (string)context.Items[ItemKey];
        return Task.CompletedTask;
    }
}
