using Microsoft.AspNetCore.Diagnostics;
using Cart.API.DTOs;
using Cart.API.Exceptions;

namespace Cart.API.ExceptionHandlers;

/// <summary>
/// ExternalServiceException → 500 (CRT-005). No es un error de negocio sino una falla al comunicarse con Products.API, por eso se loguea como Error.
/// </summary>
public class ExternalServiceExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ExternalServiceExceptionHandler> _logger;

    public ExternalServiceExceptionHandler(ILogger<ExternalServiceExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        // Si no es la excepción que este handler sabe manejar, devuelve false
        // y ASP.NET prueba con el siguiente handler registrado.
        var domainException = exception as ExternalServiceException;
        if (domainException == null)
        {
            return false;
        }

        _logger.LogError("Error {ErrorCode} (500): {ErrorMessage}", domainException.ErrorCode, domainException.Message);

        ErrorResponse error = ErrorResponseFactory.Create(context, StatusCodes.Status500InternalServerError,
            domainException.ErrorCode, domainException.Message, null);
        await ErrorResponseFactory.WriteAsync(context, error, cancellationToken);
        return true;
    }
}
