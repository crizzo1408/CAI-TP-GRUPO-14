using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MiniApi.Exceptions;
using MiniApi.Models;

namespace MiniApi.ExceptionHandlers;

public sealed class BusinessRuleExceptionHandler : IExceptionHandler
{
    private readonly ILogger<BusinessRuleExceptionHandler> _logger;

    public BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        BusinessRuleException? businessException = exception as BusinessRuleException;
        if (businessException == null)
        {
            return false;
        }

        ApiErrorResponse response = ErrorResponseFactory.Create(
            context, businessException.StatusCode, businessException.ErrorCode, businessException.Message);

        _logger.LogWarning("Error de negocio {ErrorCode} en {Endpoint}: {ErrorMessage}. CorrelationId: {CorrelationId}",
            response.ErrorCode, response.Instance, response.ErrorMessage, response.CorrelationId);

        context.Response.StatusCode = businessException.StatusCode;
        await context.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}