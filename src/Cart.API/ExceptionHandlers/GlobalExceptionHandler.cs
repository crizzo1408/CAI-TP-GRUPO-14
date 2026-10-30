using Microsoft.AspNetCore.Diagnostics;
using Cart.API.Constants;

namespace Cart.API.ExceptionHandlers;

/// <summary>
/// Red de seguridad: atrapa cualquier excepción que no manejó otro handler y responde 500 (CRT-005).
/// Se registra último. Nunca expone el stack trace; el detalle depende del entorno
/// (ErrorHandling:IncludeExceptionDetails en appsettings).
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IConfiguration configuration) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        // Errores inesperados → Error (el stack trace queda solo en el log, no en la respuesta)
        logger.LogError(exception, "Error inesperado {ErrorCode}: {ExceptionMessage}", ErrorCodes.CRT_005, exception.Message);

        var includeDetails = configuration.GetValue<bool>("ErrorHandling:IncludeExceptionDetails");
        var detail = includeDetails ? exception.Message : null;

        var error = ErrorResponseFactory.Create(context, StatusCodes.Status500InternalServerError,
            ErrorCodes.CRT_005, ErrorMessages.CRT_005, detail);
        await ErrorResponseFactory.WriteAsync(context, error, cancellationToken);
        return true;
    }
}
