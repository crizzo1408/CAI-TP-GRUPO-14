using Microsoft.AspNetCore.Diagnostics;
using MiniApi.Exceptions;

namespace MiniApi.ExceptionHandlers;

public sealed class NotFoundExceptionHandler : IExceptionHandler
{
    private readonly ILogger<NotFoundExceptionHandler> _logger;

    public NotFoundExceptionHandler(
        ILogger<NotFoundExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var notFoundException = exception as NotFoundException;

        if (notFoundException == null)
        {
            return false;
        }

        _logger.LogWarning(
            "Error {ErrorCode} en {Endpoint}. CorrelationId: {CorrelationId}",
            notFoundException.ErrorCode,
            context.Request.Path,
            context.TraceIdentifier);

        var response = ErrorResponseFactory.Create(
            context,
            StatusCodes.Status404NotFound,
            notFoundException.ErrorCode,
            notFoundException.Message);

        context.Response.StatusCode = StatusCodes.Status404NotFound;

        await context.Response.WriteAsJsonAsync(
            response,
            cancellationToken);

        return true;
    }
}