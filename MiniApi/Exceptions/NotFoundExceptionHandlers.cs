using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MiniApi.Exceptions;
using MiniApi.Models;

namespace MiniApi.ExceptionHandlers;


public sealed class NotFoundExceptionHandler : IExceptionHandler
{
    private readonly ILogger<NotFoundExceptionHandler> _logger;

    public NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        NotFoundException? notFoundException = exception as NotFoundException;
        if (notFoundException == null)
        {
            return false;
        }

        ApiErrorResponse response = ErrorResponseFactory.Create(
            context, StatusCodes.Status404NotFound, notFoundException.ErrorCode, notFoundException.Message);

      
        _logger.LogWarning("Error de negocio {ErrorCode} en {Endpoint}: {ErrorMessage}. CorrelationId: {CorrelationId}",
            response.ErrorCode, response.Instance, response.ErrorMessage, response.CorrelationId);

        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}