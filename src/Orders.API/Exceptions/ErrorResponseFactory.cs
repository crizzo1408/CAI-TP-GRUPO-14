using Microsoft.AspNetCore.Http;
using MiniApi.Models;

namespace MiniApi.ExceptionHandlers;


public static class ErrorResponseFactory
{
    private const string CorrelationHeader = "X-Correlation-Id";

    public static ApiErrorResponse Create(HttpContext context, int status, string errorCode, string errorMessage)
    {
        ApiErrorResponse response = new ApiErrorResponse();
        response.Status = status;
        response.ErrorCode = errorCode;
        response.ErrorMessage = errorMessage;
        response.CorrelationId = GetCorrelationId(context);

        if (context.Request.Path.Value != null)
        {
            response.Instance = context.Request.Path.Value;
        }

        if (status == StatusCodes.Status400BadRequest)
        {
            response.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
            response.Title = "Bad Request";
            response.Detail = "La solicitud no es válida.";
        }
        else if (status == StatusCodes.Status404NotFound)
        {
            response.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
            response.Title = "Not Found";
            response.Detail = "El recurso solicitado no fue encontrado.";
        }
        else if (status == StatusCodes.Status409Conflict)
        {
            response.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.9";
            response.Title = "Conflict";
            response.Detail = "No se puede modificar el estado.";
        }
        else if (status == StatusCodes.Status422UnprocessableEntity)
        {
            response.Type = "https://tools.ietf.org/html/rfc4918#section-11.2";
            response.Title = "Unprocessable Entity";
            response.Detail = "No se puede procesar la solicitud.";
        }
        else
        {
            response.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
            response.Title = "Internal Server Error";
            response.Detail = "Ocurrió un error interno.";
        }

        return response;
    }


    public static string GetCorrelationId(HttpContext context)
    {
        string valor = context.Request.Headers[CorrelationHeader].ToString();
        if (string.IsNullOrEmpty(valor))
        {
            valor = context.Response.Headers[CorrelationHeader].ToString();
        }
        if (string.IsNullOrEmpty(valor))
        {
            valor = context.TraceIdentifier;
        }
        return valor;
    }
}