using Users.API.Models;

namespace Users.API.Data;

/// <summary>
/// Acceso a datos de usuarios. El service solo conoce esta interfaz, no SQL.
/// </summary>
public interface IUserRepository
{
    /// <summary>Devuelve el usuario con ese email, o null si no existe.</summary>
    Task<User> GetByEmailAsync(string email);

    /// <summary>Devuelve el usuario con ese id, o null si no existe.</summary>
    Task<User> GetByIdAsync(Guid id);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByIdAsync(Guid id);
    Task CreateAsync(User user);
    Task UpdateLoginStateAsync(Guid id, bool activo, int intentosFallidos);
}
