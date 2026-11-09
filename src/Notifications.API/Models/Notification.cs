namespace Notifications.API.Models;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Mensaje { get; set; }
    public string Tipo { get; set; }
    public string Estado { get; set; }
    public DateTime FechaEnvio { get; set; }
}
