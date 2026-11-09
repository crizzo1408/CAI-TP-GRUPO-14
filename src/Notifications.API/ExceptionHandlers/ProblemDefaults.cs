namespace Notifications.API.ExceptionHandlers;

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

public static class ProblemDefaults
{
    public static ProblemInfo For(int statusCode)
    {
        switch (statusCode)
        {
            case 400:
                return new ProblemInfo(
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    "Bad Request",
                    "La solicitud contiene datos inválidos.");
            case 404:
                return new ProblemInfo(
                    "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    "Not Found",
                    "El recurso solicitado no fue encontrado.");
            default:
                return new ProblemInfo(
                    "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    "Internal Server Error",
                    "Ocurrió un error interno en el servidor.");
        }
    }
}
