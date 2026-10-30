using Users.API.Models;

namespace Users.API.Data;

/// <summary>
/// Acceso a datos de usuarios. El service solo conoce esta interfaz, no SQL.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
    Task CreateAsync(User user);
    Task UpdateLoginStateAsync(Guid id, bool activo, int intentosFallidos);
}
