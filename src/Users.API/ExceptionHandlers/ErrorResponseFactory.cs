using Users.API.DTOs;
using Users.API.Middleware;

namespace Users.API.ExceptionHandlers;

/// <summary>
/// Arma el ErrorResponse con el formato de la consigna y lo escribe en la respuesta.
/// </summary>
public static class ErrorResponseFactory
{
    public static ErrorResponse Create(HttpContext context, int statusCode, string errorCode, string errorMessage, string? detail = null)
    {
        var problem = ProblemDefaults.For(statusCode);
        return new ErrorResponse
        {
            Type = problem.Type,
            Title = problem.Title,
            Status = statusCode,
            Detail = detail ?? problem.Detail,
            Instance = context.Request.Path.Value ?? string.Empty,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            CorrelationId = context.Items[CorrelationIdMiddleware.ItemKey] as string
        };
    }

    public static async Task WriteAsync(HttpContext context, ErrorResponse error, CancellationToken cancellationToken)
    {
        context.Response.StatusCode = error.Status;
        await context.Response.WriteAsJsonAsync(error, options: null, contentType: "application/problem+json", cancellationToken);
    }
}
