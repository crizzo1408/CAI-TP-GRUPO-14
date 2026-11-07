namespace Cart.API.Swagger;

/// <summary>
/// Agrega en Swagger un ejemplo de respuesta de error para un endpoint.
/// Se puede repetir para mostrar varios errorCode con el mismo status (ej: 404 CRT-001 y CRT-002).
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class SwaggerErrorExampleAttribute : Attribute
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    public string ErrorMessage { get; }

    public SwaggerErrorExampleAttribute(int statusCode, string errorCode, string errorMessage)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }
}
