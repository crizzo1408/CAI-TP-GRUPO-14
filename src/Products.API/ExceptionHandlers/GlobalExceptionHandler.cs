using Microsoft.AspNetCore.Diagnostics;
using MiniApi.Exceptions;

namespace MiniApi.ExceptionHandlers;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Error inesperado {ErrorCode} en {Endpoint}. " +
            "CorrelationId: {CorrelationId}",
            ErrorCodes.InternalProductError,
            context.Request.Path,
            context.TraceIdentifier);

        var response = ErrorResponseFactory.Create(
            context,
            StatusCodes.Status500InternalServerError,
            ErrorCodes.InternalProductError,
            "Error interno al procesar el producto.");

        context.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(
            response,
            cancellationToken);

        return true;
    }
}