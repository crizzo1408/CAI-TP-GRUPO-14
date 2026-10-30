namespace Users.API.ExceptionHandlers;

/// <summary>
/// Valores estándar de ProblemDetails (type, title, detail) para cada status HTTP.
/// Los usan los exception handlers y los ejemplos de Swagger, así siempre coinciden.
/// </summary>
public static class ProblemDefaults
{
    public record Problem(string Type, string Title, string Detail);

    public static Problem For(int statusCode) => statusCode switch
    {
        400 => new("https://tools.ietf.org/html/rfc7231#section-6.5.1", "Bad Request", "La solicitud contiene datos inválidos."),
        401 => new("https://tools.ietf.org/html/rfc7235#section-3.1", "Unauthorized", "Las credenciales no son válidas."),
        403 => new("https://tools.ietf.org/html/rfc7231#section-6.5.3", "Forbidden", "El acceso está prohibido."),
        404 => new("https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found", "El recurso solicitado no fue encontrado."),
        409 => new("https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict", "Ya existe un recurso con esos datos."),
        422 => new("https://tools.ietf.org/html/rfc4918#section-11.2", "Unprocessable Entity", "No se puede procesar la solicitud."),
        _ => new("https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error", "Ocurrió un error interno en el servidor.")
    };
}
