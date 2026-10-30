namespace Users.API.DTOs;

/// <summary>
/// Datos del usuario autenticado (respuesta de POST /api/users/login).
/// </summary>
public class LoginResponse
{
    /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
    public Guid Id { get; set; }

    /// <example>María</example>
    public string Nombre { get; set; } = string.Empty;

    /// <example>González</example>
    public string Apellido { get; set; } = string.Empty;

    /// <example>maria@email.com</example>
    public string Email { get; set; } = string.Empty;
}
