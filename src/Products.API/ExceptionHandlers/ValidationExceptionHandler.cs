using Microsoft.AspNetCore.Diagnostics;
using MiniApi.Exceptions;

namespace MiniApi.ExceptionHandlers;

public sealed class ValidationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ValidationExceptionHandler> _logger;

    public ValidationExceptionHandler(
        ILogger<ValidationExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var validationException = exception as ValidationException;

        if (validationException == null)
        {
            return false;
        }

        _logger.LogWarning(
            "Error {ErrorCode} en {Endpoint}. CorrelationId: {CorrelationId}",
            validationException.ErrorCode,
            context.Request.Path,
            context.TraceIdentifier);

        var response = ErrorResponseFactory.Create(
            context,
            StatusCodes.Status400BadRequest,
            validationException.ErrorCode,
            validationException.Message);

        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        await context.Response.WriteAsJsonAsync(
            response,
            cancellationToken);

        return true;
    }
}