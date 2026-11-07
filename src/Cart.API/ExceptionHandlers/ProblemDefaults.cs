namespace Cart.API.ExceptionHandlers;

/// <summary>
/// Datos estándar de ProblemDetails (type, title, detail) para un status HTTP.
/// </summary>
public class ProblemInfo
{
    public string Type { get; set; }
    public string Title { get; set; }
    public string Detail { get; set; }

    public ProblemInfo(string type, string title, string detail)
    {
        Type = type;
        Title = title;
        Detail = detail;
    }
}

/// <summary>
/// Devuelve los valores de ProblemDetails para cada status HTTP.
/// Los usan los exception handlers y los ejemplos de Swagger, así siempre coinciden.
/// </summary>
public static class ProblemDefaults
{
    public static ProblemInfo For(int statusCode)
    {
        switch (statusCode)
        {
            case 400:
                return new ProblemInfo("https://tools.ietf.org/html/rfc7231#section-6.5.1", "Bad Request", "La solicitud contiene datos inválidos.");
            case 401:
                return new ProblemInfo("https://tools.ietf.org/html/rfc7235#section-3.1", "Unauthorized", "Las credenciales no son válidas.");
            case 403:
                return new ProblemInfo("https://tools.ietf.org/html/rfc7231#section-6.5.3", "Forbidden", "El acceso está prohibido.");
            case 404:
                return new ProblemInfo("https://tools.ietf.org/html/rfc7231#section-6.5.4", "Not Found", "El recurso solicitado no fue encontrado.");
            case 409:
                return new ProblemInfo("https://tools.ietf.org/html/rfc7231#section-6.5.9", "Conflict", "Ya existe un recurso con esos datos.");
            case 422:
                return new ProblemInfo("https://tools.ietf.org/html/rfc4918#section-11.2", "Unprocessable Entity", "No se puede procesar la solicitud.");
            default:
                return new ProblemInfo("https://tools.ietf.org/html/rfc7231#section-6.6.1", "Internal Server Error", "Ocurrió un error interno en el servidor.");
        }
    }
}
