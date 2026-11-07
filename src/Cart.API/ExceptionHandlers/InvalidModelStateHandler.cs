using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Cart.API.Constants;
using Cart.API.DTOs;

namespace Cart.API.ExceptionHandlers;

/// <summary>
/// Cuando falla la validación por Data Annotations ([Required], [MaxLength], etc.)
/// o el JSON viene mal formado, [ApiController] corta el request antes de llegar al controller.
/// Esta clase reemplaza la respuesta 400 por defecto por el formato de la consigna (CRT-004),
/// con todos los problemas separados por punto y coma.
/// </summary>
public static class InvalidModelStateHandler
{
    public static IActionResult CreateResponse(ActionContext context)
    {
        // 1. Juntar todos los mensajes de error, sin repetidos
        var mensajes = new List<string>();
        foreach (var entry in context.ModelState)
        {
            string campo = entry.Key;
            ModelStateEntry estado = entry.Value;

            foreach (ModelError error in estado.Errors)
            {
                string mensaje = Traducir(campo, error);
                if (!mensajes.Contains(mensaje))
                {
                    mensajes.Add(mensaje);
                }
            }
        }

        // 2. Unirlos como "a; b; c." (se quita el punto final de cada uno para no dejar ".;")
        string errorMessage = ErrorMessages.CRT_004;
        if (mensajes.Count > 0)
        {
            var sinPuntoFinal = new List<string>();
            foreach (string mensaje in mensajes)
            {
                sinPuntoFinal.Add(mensaje.TrimEnd('.'));
            }
            errorMessage = string.Join("; ", sinPuntoFinal) + ".";
        }

        // 3. Loguear como Warning (es un error de negocio)
        ILoggerFactory loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        ILogger logger = loggerFactory.CreateLogger("InvalidModelStateHandler");
        logger.LogWarning("Error {ErrorCode} (400): {ErrorMessage}", ErrorCodes.CRT_004, errorMessage);

        // 4. Armar la respuesta con el formato de la consigna
        ErrorResponse response = ErrorResponseFactory.Create(context.HttpContext, StatusCodes.Status400BadRequest,
            ErrorCodes.CRT_004, errorMessage, null);

        var result = new ObjectResult(response);
        result.StatusCode = StatusCodes.Status400BadRequest;
        return result;
    }

    /// <summary>
    /// Los mensajes de las Data Annotations ya están en español.
    /// Los que genera ASP.NET (JSON inválido, body vacío) vienen en inglés, así que se reemplazan.
    /// </summary>
    private static string Traducir(string campo, ModelError error)
    {
        if (campo.StartsWith("$") || error.Exception != null)
        {
            return "El cuerpo de la solicitud no es un JSON válido o tiene campos con un tipo incorrecto.";
        }

        if (string.IsNullOrEmpty(campo) || campo.ToLower() == "request")
        {
            return "El cuerpo de la solicitud es obligatorio.";
        }

        if (string.IsNullOrWhiteSpace(error.ErrorMessage))
        {
            return "El campo '" + campo + "' es inválido.";
        }

        return error.ErrorMessage;
    }
}
