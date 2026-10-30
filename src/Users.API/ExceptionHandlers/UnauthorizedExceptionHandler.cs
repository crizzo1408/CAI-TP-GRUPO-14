using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

/// <summary>UnauthorizedException → 401.</summary>
public class UnauthorizedExceptionHandler(ILogger<UnauthorizedExceptionHandler> logger)
    : DomainExceptionHandlerBase<UnauthorizedException>(logger)
{
    protected override int StatusCode => StatusCodes.Status401Unauthorized;
}
