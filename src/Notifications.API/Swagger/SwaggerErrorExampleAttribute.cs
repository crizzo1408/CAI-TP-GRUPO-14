namespace Notifications.API.Swagger;

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
