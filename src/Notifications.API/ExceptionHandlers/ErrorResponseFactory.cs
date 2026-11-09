using System.Text.Json;
using Notifications.API.DTOs;
using Notifications.API.Middleware;

namespace Notifications.API.ExceptionHandlers;

public static class ErrorResponseFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    public static ErrorResponse Create(
        HttpContext context,
        int statusCode,
        string errorCode,
        string errorMessage,
        string detail)
    {
        ProblemInfo problem = ProblemDefaults.For(statusCode);

        var error = new ErrorResponse();
        error.Type = problem.Type;
        error.Title = problem.Title;
        error.Status = statusCode;
        error.Detail = detail ?? problem.Detail;
        error.Instance = context.Request.Path.ToString();
        error.ErrorCode = errorCode;
        error.ErrorMessage = errorMessage;
        error.CorrelationId = context.Items[CorrelationIdMiddleware.ItemKey] as string;
        return error;
    }

    public static async Task WriteAsync(
        HttpContext context,
        ErrorResponse error,
        CancellationToken cancellationToken)
    {
        context.Response.StatusCode = error.Status;
        context.Response.ContentType = "application/problem+json";
        string json = JsonSerializer.Serialize(error, JsonOptions);
        await context.Response.WriteAsync(json, cancellationToken);
    }
}
