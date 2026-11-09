namespace Notifications.API.DTOs;

/// <summary>Formato común de todas las respuestas de error.</summary>
public class ErrorResponse
{
    public string Type { get; set; }
    public string Title { get; set; }
    public int Status { get; set; }
    public string Detail { get; set; }
    public string Instance { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public string CorrelationId { get; set; }
}
