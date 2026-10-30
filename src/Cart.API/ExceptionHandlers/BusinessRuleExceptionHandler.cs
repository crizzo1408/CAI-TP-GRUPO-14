using Cart.API.Exceptions;

namespace Cart.API.ExceptionHandlers;

/// <summary>BusinessRuleException → 422.</summary>
public class BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger)
    : DomainExceptionHandlerBase<BusinessRuleException>(logger)
{
    protected override int StatusCode => StatusCodes.Status422UnprocessableEntity;
}
