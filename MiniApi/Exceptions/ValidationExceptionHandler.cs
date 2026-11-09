using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MiniApi.Exceptions;
using MiniApi.Models;

namespace MiniApi.ExceptionHandlers;


public sealed class ValidationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ValidationExceptionHandler> _logger;

    public ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        ValidationException? validationException = exception as ValidationException;
        BadHttpRequestException? badRequestException = exception as BadHttpRequestException;

        if (validationException == null && badRequestException == null)
        {
            return false;
        }

        string errorCode;
        string errorMessage;

        if (validationException != null)
        {
            errorCode = validationException.ErrorCode;
            errorMessage = validationException.Message;
        }
        else
        {
            errorCode = OrderErrorCodes.InvalidData;
            errorMessage = "Los datos de la orden son inválidos: el body no tiene un formato válido.";
        }

        ApiErrorResponse response = ErrorResponseFactory.Create(
            context, StatusCodes.Status400BadRequest, errorCode, errorMessage);

        _logger.LogWarning("Error de negocio {ErrorCode} en {Endpoint}: {ErrorMessage}. CorrelationId: {CorrelationId}",
            response.ErrorCode, response.Instance, response.ErrorMessage, response.CorrelationId);

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}