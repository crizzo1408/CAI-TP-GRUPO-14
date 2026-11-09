using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Users.API.Models;

namespace Users.API.Data;

/// <summary>
/// Fila de la tabla users tal como viene de SQLite (todo TEXT o INTEGER).
/// Dapper llena esta clase y después se convierte a User con los tipos reales.
/// </summary>
public class UserRow
{
    public string Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string FechaRegistro { get; set; }
    public long Activo { get; set; }
    public long IntentosFallidos { get; set; }
}

/// <summary>
/// Implementación con SQLite + Dapper (mismo patrón que ItemRepository de la plantilla).
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IConfiguration _configuration;

    public UserRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_configuration.GetConnectionString("DefaultConnection"));
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        using var connection = CreateConnection();
        UserRow row = await connection.QuerySingleOrDefaultAsync<UserRow>(@"
            SELECT id                AS Id,
                   nombre            AS Nombre,
                   apellido          AS Apellido,
                   email             AS Email,
                   password_hash     AS PasswordHash,
                   fecha_registro    AS FechaRegistro,
                   activo            AS Activo,
                   intentos_fallidos AS IntentosFallidos
            FROM users
            WHERE email = @Email",
            new { Email = email });

        // Si no existe el email, Dapper devuelve null
        if (row == null)
        {
            return null;
        }

        return ConvertirAUser(row);
    }

    public async Task<User> GetByIdAsync(Guid id)
    {
        using var connection = CreateConnection();
        UserRow row = await connection.QuerySingleOrDefaultAsync<UserRow>(@"
            SELECT id                AS Id,
                   nombre            AS Nombre,
                   apellido          AS Apellido,
                   email             AS Email,
                   password_hash     AS PasswordHash,
                   fecha_registro    AS FechaRegistro,
                   activo            AS Activo,
                   intentos_fallidos AS IntentosFallidos
            FROM users
            WHERE id = @Id",
            new { Id = id.ToString() });

        // Si no existe el id, Dapper devuelve null
        if (row == null)
        {
            return null;
        }

        return ConvertirAUser(row);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        using var connection = CreateConnection();
        long count = await connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM users WHERE email = @Email",
            new { Email = email });
        return count > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        using var connection = CreateConnection();
        long count = await connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM users WHERE id = @Id",
            new { Id = id.ToString() });
        return count > 0;
    }

    public async Task CreateAsync(User user)
    {
        int activo = 0;
        if (user.Activo)
        {
            activo = 1;
        }

        using var connection = CreateConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO users (id, nombre, apellido, email, password_hash, fecha_registro, activo, intentos_fallidos)
            VALUES (@Id, @Nombre, @Apellido, @Email, @PasswordHash, @FechaRegistro, @Activo, @IntentosFallidos);",
            new
            {
                Id = user.Id.ToString(),
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                FechaRegistro = user.FechaRegistro.ToString("o"),
                Activo = activo,
                IntentosFallidos = user.IntentosFallidos
            });
    }

    public async Task UpdateLoginStateAsync(Guid id, bool activo, int intentosFallidos)
    {
        int activoNumero = 0;
        if (activo)
        {
            activoNumero = 1;
        }

        using var connection = CreateConnection();
        await connection.ExecuteAsync(@"
            UPDATE users
            SET activo = @Activo, intentos_fallidos = @IntentosFallidos
            WHERE id = @Id",
            new { Id = id.ToString(), Activo = activoNumero, IntentosFallidos = intentosFallidos });
    }

    /// <summary>Convierte la fila de SQLite (texto y números) a la entidad User.</summary>
    private static User ConvertirAUser(UserRow row)
    {
        var user = new User();
        user.Id = Guid.Parse(row.Id);
        user.Nombre = row.Nombre;
        user.Apellido = row.Apellido;
        user.Email = row.Email;
        user.PasswordHash = row.PasswordHash;
        user.FechaRegistro = DateTime.Parse(row.FechaRegistro, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        user.Activo = row.Activo == 1;
        user.IntentosFallidos = (int)row.IntentosFallidos;
        return user;
    }
}
