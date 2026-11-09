using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers;

public class NotFoundExceptionHandler : IExceptionHandler
{
    private readonly ILogger<NotFoundExceptionHandler> _logger;

    public NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var domainException = exception as NotFoundException;
        if (domainException == null)
        {
            return false;
        }

        _logger.LogWarning(
            "Error {ErrorCode} (404): {ErrorMessage}",
            domainException.ErrorCode,
            domainException.Message);

        var error = ErrorResponseFactory.Create(
            context,
            StatusCodes.Status404NotFound,
            domainException.ErrorCode,
            domainException.Message,
            null);
        await ErrorResponseFactory.WriteAsync(context, error, cancellationToken);
        return true;
    }
}
