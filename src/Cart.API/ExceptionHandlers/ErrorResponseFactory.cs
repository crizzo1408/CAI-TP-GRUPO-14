using System.Text.Json;
using Cart.API.DTOs;
using Cart.API.Middleware;

namespace Cart.API.ExceptionHandlers;

/// <summary>
/// Arma el ErrorResponse con el formato de la consigna y lo escribe en la respuesta.
/// </summary>
public static class ErrorResponseFactory
{
    // Web = nombres en camelCase (errorCode, errorMessage, ...)
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    // Si detail es null se usa el detail estándar del status
    public static ErrorResponse Create(HttpContext context, int statusCode, string errorCode, string errorMessage, string detail)
    {
        ProblemInfo problem = ProblemDefaults.For(statusCode);

        var error = new ErrorResponse();
        error.Type = problem.Type;
        error.Title = problem.Title;
        error.Status = statusCode;
        error.Detail = problem.Detail;
        error.Instance = context.Request.Path.ToString();
        error.ErrorCode = errorCode;
        error.ErrorMessage = errorMessage;
        error.CorrelationId = context.Items[CorrelationIdMiddleware.ItemKey] as string;

        if (detail != null)
        {
            error.Detail = detail;
        }

        return error;
    }

    public static async Task WriteAsync(HttpContext context, ErrorResponse error, CancellationToken cancellationToken)
    {
        context.Response.StatusCode = error.Status;
        context.Response.ContentType = "application/problem+json";

        string json = JsonSerializer.Serialize(error, JsonOptions);
        await context.Response.WriteAsync(json, cancellationToken);
    }
}
