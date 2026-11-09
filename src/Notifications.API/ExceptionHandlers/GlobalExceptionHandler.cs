using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.Constants;

namespace Notifications.API.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IConfiguration _configuration;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Error inesperado {ErrorCode}: {ExceptionMessage}",
            ErrorCodes.NTF_004,
            exception.Message);

        string detail = null;
        if (_configuration.GetValue<bool>("ErrorHandling:IncludeExceptionDetails"))
        {
            detail = exception.Message;
        }

        var error = ErrorResponseFactory.Create(
            context,
            StatusCodes.Status500InternalServerError,
            ErrorCodes.NTF_004,
            ErrorMessages.NTF_004,
            detail);
        await ErrorResponseFactory.WriteAsync(context, error, cancellationToken);
        return true;
    }
}
