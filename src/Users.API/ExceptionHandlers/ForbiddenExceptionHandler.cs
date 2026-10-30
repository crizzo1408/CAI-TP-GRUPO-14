using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

/// <summary>ForbiddenException → 403.</summary>
public class ForbiddenExceptionHandler(ILogger<ForbiddenExceptionHandler> logger)
    : DomainExceptionHandlerBase<ForbiddenException>(logger)
{
    protected override int StatusCode => StatusCodes.Status403Forbidden;
}
