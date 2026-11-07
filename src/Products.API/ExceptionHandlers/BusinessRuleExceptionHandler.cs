using Microsoft.AspNetCore.Diagnostics;
using MiniApi.Exceptions;

namespace MiniApi.ExceptionHandlers;

public sealed class BusinessRuleExceptionHandler : IExceptionHandler
{
    private readonly ILogger<BusinessRuleExceptionHandler> _logger;

    public BusinessRuleExceptionHandler(
        ILogger<BusinessRuleExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var businessException = exception as BusinessRuleException;

        if (businessException == null)
        {
            return false;
        }

        _logger.LogWarning(
            "Error {ErrorCode} en {Endpoint}. CorrelationId: {CorrelationId}",
            businessException.ErrorCode,
            context.Request.Path,
            context.TraceIdentifier);

        var response = ErrorResponseFactory.Create(
            context,
            StatusCodes.Status409Conflict,
            businessException.ErrorCode,
            businessException.Message);

        context.Response.StatusCode = StatusCodes.Status409Conflict;

        await context.Response.WriteAsJsonAsync(
            response,
            cancellationToken);

        return true;
    }
}