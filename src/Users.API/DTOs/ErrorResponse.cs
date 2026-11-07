namespace Users.API.DTOs;

/// <summary>
/// Formato de todas las respuestas de error 4xx y 5xx (sección 3 de la consigna).
/// Son los campos estándar de ProblemDetails más errorCode y errorMessage,
/// y el correlationId del request.
/// </summary>
public class ErrorResponse
{
    /// <example>https://tools.ietf.org/html/rfc7231#section-6.5.1</example>
    public string Type { get; set; }

    /// <example>Bad Request</example>
    public string Title { get; set; }

    /// <example>400</example>
    public int Status { get; set; }

    /// <example>La solicitud contiene datos inválidos.</example>
    public string Detail { get; set; }

    /// <example>/api/users/register</example>
    public string Instance { get; set; }

    /// <example>USR-002</example>
    public string ErrorCode { get; set; }

    /// <example>Los datos del usuario son inválidos.</example>
    public string ErrorMessage { get; set; }

    /// <summary>Identificador del request, el mismo que aparece en los logs.</summary>
    /// <example>0f8fad5b-d9cb-469f-a165-70867728950e</example>
    public string CorrelationId { get; set; }
}
