using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers;

public class ValidationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ValidationExceptionHandler> _logger;

    public ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var domainException = exception as ValidationException;
        if (domainException == null)
        {
            return false;
        }

        _logger.LogWarning(
            "Error {ErrorCode} (400): {ErrorMessage}",
            domainException.ErrorCode,
            domainException.Message);

        var error = ErrorResponseFactory.Create(
            context,
            StatusCodes.Status400BadRequest,
            domainException.ErrorCode,
            domainException.Message,
            null);
        await ErrorResponseFactory.WriteAsync(context, error, cancellationToken);
        return true;
    }
}
