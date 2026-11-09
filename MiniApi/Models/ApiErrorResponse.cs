namespace MiniApi.Models;


public class ApiErrorResponse
{
    /// <example>https://tools.ietf.org/html/rfc7231#section-6.5.4</example>
    public string Type { get; set; } = string.Empty;

    ///Not Found
    public string Title { get; set; } = string.Empty;

    ///404
    public int Status { get; set; }

    ///El recurso solicitado no fue encontrado
    public string Detail { get; set; } = string.Empty;

   
    public string Instance { get; set; } = string.Empty;

    /// <example>ORD-001</example>
    public string ErrorCode { get; set; } = string.Empty;

    ///Orden no encontrada
    public string ErrorMessage { get; set; } = string.Empty;

    /// prueba-123
    public string CorrelationId { get; set; } = string.Empty;
}