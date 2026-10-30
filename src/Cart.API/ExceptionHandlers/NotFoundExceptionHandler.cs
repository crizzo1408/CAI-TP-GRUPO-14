using Cart.API.Exceptions;

namespace Cart.API.ExceptionHandlers;

/// <summary>NotFoundException → 404.</summary>
public class NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger)
    : DomainExceptionHandlerBase<NotFoundException>(logger)
{
    protected override int StatusCode => StatusCodes.Status404NotFound;
}
