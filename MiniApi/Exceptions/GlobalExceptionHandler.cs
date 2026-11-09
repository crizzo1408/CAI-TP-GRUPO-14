using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MiniApi.Exceptions;
using MiniApi.Models;

namespace MiniApi.ExceptionHandlers;


public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IConfiguration _configuration;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        ApiErrorResponse response = ErrorResponseFactory.Create(
            context, StatusCodes.Status500InternalServerError, OrderErrorCodes.Internal,
            "Error interno al procesar la orden.");

        // Sección 5.3: errores inesperados como Error (con la excepción completa en el log)
        _logger.LogError(exception, "Error inesperado {ErrorCode} en {Endpoint}. CorrelationId: {CorrelationId}",
            response.ErrorCode, response.Instance, response.CorrelationId);

        // Sección 5.2: más detalle en Development, mensaje genérico en Production.
        // El stack trace no se expone nunca, en ningún entorno.
        bool incluirDetalle = _configuration.GetValue<bool>("ErrorHandling:IncludeExceptionDetails");
        if (incluirDetalle)
        {
            response.Detail = exception.GetType().Name + ": " + exception.Message;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}
