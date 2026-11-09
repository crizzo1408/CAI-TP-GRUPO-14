using Microsoft.AspNetCore.Diagnostics;
using Users.API.DTOs;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

/// <summary>
/// NotFoundException → 404. Error de negocio: se loguea como Warning.
/// </summary>
public class NotFoundExceptionHandler : IExceptionHandler
{
    private readonly ILogger<NotFoundExceptionHandler> _logger;

    public NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        // Si no es la excepción que este handler sabe manejar, devuelve false
        // y ASP.NET prueba con el siguiente handler registrado.
        var domainException = exception as NotFoundException;
        if (domainException == null)
        {
            return false;
        }

        _logger.LogWarning("Error {ErrorCode} (404): {ErrorMessage}", domainException.ErrorCode, domainException.Message);

        ErrorResponse error = ErrorResponseFactory.Create(context, StatusCodes.Status404NotFound,
            domainException.ErrorCode, domainException.Message, null);
        await ErrorResponseFactory.WriteAsync(context, error, cancellationToken);
        return true;
    }
}
