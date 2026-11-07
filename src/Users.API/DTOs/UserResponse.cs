namespace Users.API.DTOs;

/// <summary>
/// Usuario registrado (respuesta de POST /api/users/register).
/// No incluye PasswordHash.
/// </summary>
public class UserResponse
{
    /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
    public Guid Id { get; set; }

    /// <example>María</example>
    public string Nombre { get; set; }

    /// <example>González</example>
    public string Apellido { get; set; }

    /// <example>maria@email.com</example>
    public string Email { get; set; }

    /// <example>2024-03-10T09:00:00Z</example>
    public DateTime FechaRegistro { get; set; }

    /// <example>true</example>
    public bool Activo { get; set; }
}
