using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Users.API.Models;

namespace Users.API.Data;

/// <summary>
/// Implementación con SQLite + Dapper (mismo patrón que ItemRepository de la plantilla).
/// </summary>
public class UserRepository(IConfiguration configuration) : IUserRepository
{
    private SqliteConnection CreateConnection() =>
        new(configuration.GetConnectionString("DefaultConnection"));

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<UserRow>("""
            SELECT id                AS Id,
                   nombre            AS Nombre,
                   apellido          AS Apellido,
                   email             AS Email,
                   password_hash     AS PasswordHash,
                   fecha_registro    AS FechaRegistro,
                   activo            AS Activo,
                   intentos_fallidos AS IntentosFallidos
            FROM users
            WHERE email = @email
            """, new { email });

        return row?.ToModel();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        using var connection = CreateConnection();
        var count = await connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM users WHERE email = @email", new { email });
        return count > 0;
    }

    public async Task CreateAsync(User user)
    {
        using var connection = CreateConnection();
        await connection.ExecuteAsync("""
            INSERT INTO users (id, nombre, apellido, email, password_hash, fecha_registro, activo, intentos_fallidos)
            VALUES (@Id, @Nombre, @Apellido, @Email, @PasswordHash, @FechaRegistro, @Activo, @IntentosFallidos);
            """,
            new
            {
                Id = user.Id.ToString(),
                user.Nombre,
                user.Apellido,
                user.Email,
                user.PasswordHash,
                FechaRegistro = user.FechaRegistro.ToString("o"),
                Activo = user.Activo ? 1 : 0,
                user.IntentosFallidos
            });
    }

    public async Task UpdateLoginStateAsync(Guid id, bool activo, int intentosFallidos)
    {
        using var connection = CreateConnection();
        await connection.ExecuteAsync("""
            UPDATE users
            SET activo = @Activo, intentos_fallidos = @IntentosFallidos
            WHERE id = @Id
            """,
            new { Id = id.ToString(), Activo = activo ? 1 : 0, IntentosFallidos = intentosFallidos });
    }

    /// <summary>
    /// Fila tal como viene de SQLite (todo TEXT/INTEGER). Se convierte a User con tipos reales.
    /// </summary>
    private sealed class UserRow
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FechaRegistro { get; set; } = string.Empty;
        public long Activo { get; set; }
        public long IntentosFallidos { get; set; }

        public User ToModel() => new()
        {
            Id = Guid.Parse(Id),
            Nombre = Nombre,
            Apellido = Apellido,
            Email = Email,
            PasswordHash = PasswordHash,
            FechaRegistro = DateTime.Parse(FechaRegistro, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            Activo = Activo == 1,
            IntentosFallidos = (int)IntentosFallidos
        };
    }
}
