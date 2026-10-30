using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

/// <summary>ValidationException → 400.</summary>
public class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
    : DomainExceptionHandlerBase<ValidationException>(logger)
{
    protected override int StatusCode => StatusCodes.Status400BadRequest;
}
