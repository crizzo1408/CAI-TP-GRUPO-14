using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

/// <summary>ConflictException → 409.</summary>
public class ConflictExceptionHandler(ILogger<ConflictExceptionHandler> logger)
    : DomainExceptionHandlerBase<ConflictException>(logger)
{
    protected override int StatusCode => StatusCodes.Status409Conflict;
}
