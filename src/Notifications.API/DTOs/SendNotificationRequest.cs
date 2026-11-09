using System.ComponentModel.DataAnnotations;

namespace Notifications.API.DTOs;

/// <summary>Datos necesarios para enviar una notificación.</summary>
public class SendNotificationRequest
{
    /// <summary>Identificador de un usuario existente en Users.API.</summary>
    /// <example>0f8fad5b-d9cb-469f-a165-70867728950e</example>
    public Guid UsuarioId { get; set; }

    /// <summary>Contenido de la notificación (máximo 500 caracteres).</summary>
    /// <example>Tu pedido fue confirmado.</example>
    [Required(ErrorMessage = "El mensaje es obligatorio.")]
    [MaxLength(500, ErrorMessage = "El mensaje no puede superar los 500 caracteres.")]
    public string Mensaje { get; set; }

    /// <summary>Canal de envío: Email, Push o SMS.</summary>
    /// <example>Email</example>
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    public string Tipo { get; set; }
}
