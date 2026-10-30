using Cart.API.Exceptions;

namespace Cart.API.ExceptionHandlers;

/// <summary>
/// ExternalServiceException → 500 (CRT-005).
/// No es un error de negocio sino una falla de infraestructura, por eso se loguea como Error.
/// </summary>
public class ExternalServiceExceptionHandler(ILogger<ExternalServiceExceptionHandler> logger)
    : DomainExceptionHandlerBase<ExternalServiceException>(logger)
{
    protected override int StatusCode => StatusCodes.Status500InternalServerError;
    protected override LogLevel LogLevel => LogLevel.Error;
}
