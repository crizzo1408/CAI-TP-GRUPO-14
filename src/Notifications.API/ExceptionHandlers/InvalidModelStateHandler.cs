using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Notifications.API.Constants;

namespace Notifications.API.ExceptionHandlers;

public static class InvalidModelStateHandler
{
    public static IActionResult CreateResponse(ActionContext context)
    {
        var messages = new List<string>();

        foreach (var entry in context.ModelState)
        {
            string field = entry.Key;
            ModelStateEntry state = entry.Value;

            foreach (ModelError error in state.Errors)
            {
                string message = Translate(field, error);
                if (!messages.Contains(message))
                {
                    messages.Add(message);
                }
            }
        }

        string errorMessage = ErrorMessages.NTF_002;
        if (messages.Count > 0)
        {
            var cleanMessages = new List<string>();
            foreach (string message in messages)
            {
                cleanMessages.Add(message.TrimEnd('.'));
            }

            errorMessage = string.Join("; ", cleanMessages) + ".";
        }

        ILoggerFactory loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        ILogger logger = loggerFactory.CreateLogger("InvalidModelStateHandler");
        logger.LogWarning("Error {ErrorCode} (400): {ErrorMessage}", ErrorCodes.NTF_002, errorMessage);

        var response = ErrorResponseFactory.Create(
            context.HttpContext,
            StatusCodes.Status400BadRequest,
            ErrorCodes.NTF_002,
            errorMessage,
            null);

        return new ObjectResult(response)
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
    }

    private static string Translate(string field, ModelError error)
    {
        if (field.StartsWith("$") || error.Exception != null)
        {
            return "El cuerpo de la solicitud no es un JSON válido o tiene campos con un tipo incorrecto.";
        }

        if (string.IsNullOrEmpty(field) || field.Equals("request", StringComparison.OrdinalIgnoreCase))
        {
            return "El cuerpo de la solicitud es obligatorio.";
        }

        if (string.IsNullOrWhiteSpace(error.ErrorMessage))
        {
            return "El campo '" + field + "' es inválido.";
        }

        return error.ErrorMessage;
    }
}
