using Microsoft.AspNetCore.Mvc;
using Users.API.Constants;

namespace Users.API.ExceptionHandlers;

/// <summary>
/// Cuando falla la validación por Data Annotations ([Required], [MaxLength], etc.)
/// o el JSON viene mal formado, [ApiController] corta el request antes de llegar al controller.
/// Esta clase reemplaza la respuesta 400 por defecto por el formato de la consigna (USR-002),
/// con todos los problemas separados por punto y coma.
/// </summary>
public static class InvalidModelStateHandler
{
    public static IActionResult CreateResponse(ActionContext context)
    {
        var mensajes = context.ModelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .SelectMany(entry => entry.Value!.Errors.Select(error => Traducir(entry.Key, error)))
            .Distinct()
            .ToList();

        // Se quita el punto final de cada mensaje para que queden "a; b; c." y no "a.; b.; c."
        var errorMessage = mensajes.Count > 0
            ? string.Join("; ", mensajes.Select(m => m.TrimEnd('.'))) + "."
            : ErrorMessages.USR_002;

        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(InvalidModelStateHandler));
        logger.LogWarning("Error de negocio {ErrorCode} (400): {ErrorMessage}", ErrorCodes.USR_002, errorMessage);

        var error = ErrorResponseFactory.Create(context.HttpContext, StatusCodes.Status400BadRequest,
            ErrorCodes.USR_002, errorMessage);

        return new ObjectResult(error)
        {
            StatusCode = StatusCodes.Status400BadRequest,
            ContentTypes = { "application/problem+json" }
        };
    }

    /// <summary>
    /// Los mensajes de las Data Annotations ya están en español.
    /// Los que genera ASP.NET (JSON inválido, body vacío) vienen en inglés, así que se reemplazan.
    /// </summary>
    private static string Traducir(string campo, Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error)
    {
        if (campo.StartsWith('$') || error.Exception is not null)
            return "El cuerpo de la solicitud no es un JSON válido o tiene campos con un tipo incorrecto.";

        if (string.IsNullOrEmpty(campo) || campo.Equals("request", StringComparison.OrdinalIgnoreCase))
            return "El cuerpo de la solicitud es obligatorio.";

        return string.IsNullOrWhiteSpace(error.ErrorMessage)
            ? $"El campo '{campo}' es inválido."
            : error.ErrorMessage;
    }
}
