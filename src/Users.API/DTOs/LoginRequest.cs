using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs;

/// <summary>
/// Credenciales para iniciar sesión.
/// </summary>
public class LoginRequest
{
    /// <summary>Email del usuario.</summary>
    /// <example>maria@email.com</example>
    [Required(ErrorMessage = "El email es obligatorio.")]
    public string? Email { get; set; }

    /// <summary>Contraseña del usuario.</summary>
    /// <example>MiPassword123!</example>
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string? Password { get; set; }
}
