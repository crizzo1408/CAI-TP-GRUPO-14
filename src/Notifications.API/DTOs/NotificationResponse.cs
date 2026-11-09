namespace Notifications.API.DTOs;

/// <summary>Notificación registrada por el servicio.</summary>
public class NotificationResponse
{
    /// <example>72d8d070-3c6a-4c32-a57a-27a5777cf63d</example>
    public Guid Id { get; set; }

    /// <example>0f8fad5b-d9cb-469f-a165-70867728950e</example>
    public Guid UsuarioId { get; set; }

    /// <example>Tu pedido fue confirmado.</example>
    public string Mensaje { get; set; }

    /// <example>Email</example>
    public string Tipo { get; set; }

    /// <example>Enviada</example>
    public string Estado { get; set; }

    /// <example>2026-11-09T14:30:00Z</example>
    public DateTime FechaEnvio { get; set; }
}
