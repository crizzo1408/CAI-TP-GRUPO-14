using Serilog.Context;

namespace MiniApi.Middleware;

public sealed class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = string.Empty;

        if (context.Request.Headers.ContainsKey(HeaderName))
        {
            correlationId =
                context.Request.Headers[HeaderName].ToString();
        }

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.TraceIdentifier = correlationId;

        context.Response.OnStarting(
            AddCorrelationIdHeader,
            context);

        using (LogContext.PushProperty(
            "CorrelationId",
            correlationId))
        {
            var endpoint =
                context.Request.Path.Value ?? string.Empty;

            _logger.LogInformation(
                "Request starting {HttpMethod} {Endpoint}",
                context.Request.Method,
                endpoint);

            await _next(context);
        }
    }

    private static Task AddCorrelationIdHeader(
        object state)
    {
        var context = (HttpContext)state;

        context.Response.Headers[HeaderName] =
            context.TraceIdentifier;

        return Task.CompletedTask;
    }
}