namespace Users.API.Models;

/// <summary>
/// Entidad de dominio User (Apéndice A de la consigna).
/// Nunca se devuelve directamente en una respuesta: para eso están los DTOs,
/// así PasswordHash no se expone nunca.
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }

    /// <summary>false cuando el usuario está bloqueado.</summary>
    public bool Activo { get; set; } = true;

    /// <summary>Se incrementa en cada login fallido y se resetea al loguearse OK.</summary>
    public int IntentosFallidos { get; set; }
}
