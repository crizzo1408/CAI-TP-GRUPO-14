using Microsoft.AspNetCore.Diagnostics;
using Users.API.Constants;

namespace Users.API.ExceptionHandlers;

/// <summary>
/// Red de seguridad: atrapa cualquier excepción que no manejó otro handler y responde 500 (USR-006).
/// Se registra último. Nunca expone el stack trace; el detalle depende del entorno
/// (ErrorHandling:IncludeExceptionDetails en appsettings).
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IConfiguration configuration) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        // Errores inesperados → Error (el stack trace queda solo en el log, no en la respuesta)
        logger.LogError(exception, "Error inesperado {ErrorCode}: {ExceptionMessage}", ErrorCodes.USR_006, exception.Message);

        var includeDetails = configuration.GetValue<bool>("ErrorHandling:IncludeExceptionDetails");
        var detail = includeDetails ? exception.Message : null;

        var error = ErrorResponseFactory.Create(context, StatusCodes.Status500InternalServerError,
            ErrorCodes.USR_006, ErrorMessages.USR_006, detail);
        await ErrorResponseFactory.WriteAsync(context, error, cancellationToken);
        return true;
    }
}
