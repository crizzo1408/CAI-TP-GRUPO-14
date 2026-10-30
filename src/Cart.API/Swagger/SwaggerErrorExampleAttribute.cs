namespace Cart.API.Swagger;

/// <summary>
/// Agrega en Swagger un ejemplo de respuesta de error para un endpoint.
/// Se puede repetir para mostrar varios errorCode con el mismo status (ej: 404 CRT-001 y CRT-002).
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class SwaggerErrorExampleAttribute(int statusCode, string errorCode, string errorMessage) : Attribute
{
    public int StatusCode { get; } = statusCode;
    public string ErrorCode { get; } = errorCode;
    public string ErrorMessage { get; } = errorMessage;
}
