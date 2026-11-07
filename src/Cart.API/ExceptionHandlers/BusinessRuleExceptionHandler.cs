using Microsoft.AspNetCore.Diagnostics;
using Cart.API.DTOs;
using Cart.API.Exceptions;

namespace Cart.API.ExceptionHandlers;

/// <summary>
/// BusinessRuleException → 422. Error de negocio: se loguea como Warning.
/// </summary>
public class BusinessRuleExceptionHandler : IExceptionHandler
{
    private readonly ILogger<BusinessRuleExceptionHandler> _logger;

    public BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        // Si no es la excepción que este handler sabe manejar, devuelve false
        // y ASP.NET prueba con el siguiente handler registrado.
        var domainException = exception as BusinessRuleException;
        if (domainException == null)
        {
            return false;
        }

        _logger.LogWarning("Error {ErrorCode} (422): {ErrorMessage}", domainException.ErrorCode, domainException.Message);

        ErrorResponse error = ErrorResponseFactory.Create(context, StatusCodes.Status422UnprocessableEntity,
            domainException.ErrorCode, domainException.Message, null);
        await ErrorResponseFactory.WriteAsync(context, error, cancellationToken);
        return true;
    }
}
