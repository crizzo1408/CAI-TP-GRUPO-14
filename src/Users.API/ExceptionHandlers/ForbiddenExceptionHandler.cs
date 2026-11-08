using Microsoft.AspNetCore.Diagnostics;
using Users.API.DTOs;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

/// <summary>
/// ForbiddenException → 403. Error de negocio: se loguea como Warning.
/// </summary>
public class ForbiddenExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ForbiddenExceptionHandler> _logger;

    public ForbiddenExceptionHandler(ILogger<ForbiddenExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        // Si no es la excepción que este handler sabe manejar, devuelve false
        // y ASP.NET prueba con el siguiente handler registrado.
        var domainException = exception as ForbiddenException;
        if (domainException == null)
        {
            return false;
        }

        _logger.LogWarning("Error {ErrorCode} (403): {ErrorMessage}", domainException.ErrorCode, domainException.Message);

        ErrorResponse error = ErrorResponseFactory.Create(context, StatusCodes.Status403Forbidden,
            domainException.ErrorCode, domainException.Message, null);
        await ErrorResponseFactory.WriteAsync(context, error, cancellationToken);
        return true;
    }
}
