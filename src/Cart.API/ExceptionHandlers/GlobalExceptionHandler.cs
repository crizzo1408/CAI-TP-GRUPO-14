using Microsoft.AspNetCore.Diagnostics;
using Cart.API.Constants;
using Cart.API.DTOs;

namespace Cart.API.ExceptionHandlers;

/// <summary>
/// Red de seguridad: atrapa cualquier excepción que no manejó otro handler y responde 500 (CRT-005).
/// Se registra último. Nunca expone el stack trace; el detalle depende del entorno
/// (ErrorHandling:IncludeExceptionDetails en appsettings).
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
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
        // Errores inesperados → Error (el stack trace queda solo en el log, no en la respuesta)
        _logger.LogError(exception, "Error inesperado {ErrorCode}: {ExceptionMessage}", ErrorCodes.CRT_005, exception.Message);

        // En Development se muestra el mensaje de la excepción; en Production, el texto genérico
        string detail = null;
        bool includeDetails = _configuration.GetValue<bool>("ErrorHandling:IncludeExceptionDetails");
        if (includeDetails)
        {
            detail = exception.Message;
        }

        ErrorResponse error = ErrorResponseFactory.Create(context, StatusCodes.Status500InternalServerError,
            ErrorCodes.CRT_005, ErrorMessages.CRT_005, detail);
        await ErrorResponseFactory.WriteAsync(context, error, cancellationToken);
        return true;
    }
}
