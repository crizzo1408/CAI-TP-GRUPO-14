using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs;

/// <summary>
/// Datos para registrar un nuevo usuario.
/// </summary>
public class RegisterUserRequest
{
    /// <summary>Nombre del usuario.</summary>
    /// <example>María</example>
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; }

    /// <summary>Apellido del usuario.</summary>
    /// <example>González</example>
    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
    public string Apellido { get; set; }

    /// <summary>Email del usuario. Debe ser único.</summary>
    /// <example>maria@email.com</example>
    [Required(ErrorMessage = "El email es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El email no puede superar los 200 caracteres.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "El email no tiene un formato válido.")]
    public string Email { get; set; }

    /// <summary>Contraseña. Mínimo 8 caracteres.</summary>
    /// <example>MiPassword123!</example>
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [MaxLength(100, ErrorMessage = "La contraseña no puede superar los 100 caracteres.")]
    public string Password { get; set; }
}
