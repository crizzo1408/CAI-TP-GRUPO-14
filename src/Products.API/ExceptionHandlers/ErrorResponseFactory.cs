using MiniApi.Models;

namespace MiniApi.ExceptionHandlers;

internal static class ErrorResponseFactory
{
    public static ApiErrorResponse Create(
        HttpContext context,
        int status,
        string errorCode,
        string errorMessage)
    {
        var response = new ApiErrorResponse
        {
            Status = status,
            Instance = context.Request.Path.Value ?? string.Empty,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            CorrelationId = context.TraceIdentifier
        };

        if (status == StatusCodes.Status400BadRequest)
        {
            response.Type =
                "https://tools.ietf.org/html/rfc7231#section-6.5.1";
            response.Title = "Bad Request";
            response.Detail = "La solicitud contiene datos inválidos.";
        }
        else if (status == StatusCodes.Status404NotFound)
        {
            response.Type =
                "https://tools.ietf.org/html/rfc7231#section-6.5.4";
            response.Title = "Not Found";
            response.Detail =
                "El recurso solicitado no fue encontrado.";
        }
        else if (status == StatusCodes.Status409Conflict)
        {
            response.Type =
                "https://tools.ietf.org/html/rfc7231#section-6.5.9";
            response.Title = "Conflict";
            response.Detail =
                "No se puede completar la operación solicitada.";
        }
        else
        {
            response.Type =
                "https://tools.ietf.org/html/rfc7231#section-6.6.1";
            response.Title = "Internal Server Error";
            response.Detail =
                "Ocurrió un error interno al procesar la solicitud.";
        }

        return response;
    }
}