using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Users.API.Models;

namespace Users.API.Data;

/// <summary>
/// Crea la tabla users en SQLite si no existe (mismo patrón que la plantilla MiniApi).
/// En Development además crea un usuario bloqueado manualmente para poder probar USR-005.
/// </summary>
public class DatabaseInitializer(
    IConfiguration configuration,
    IPasswordHasher<User> passwordHasher,
    ILogger<DatabaseInitializer> logger)
{
    public const string UsuarioBloqueadoEmail = "bloqueado@email.com";
    public const string UsuarioBloqueadoPassword = "Password123!";

    public void Initialize()
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        // Los Guid y las fechas se guardan como TEXT; los bool como INTEGER (0/1)
        connection.Execute("""
            CREATE TABLE IF NOT EXISTS users (
                id                TEXT    PRIMARY KEY,
                nombre            TEXT    NOT NULL,
                apellido          TEXT    NOT NULL,
                email             TEXT    NOT NULL UNIQUE COLLATE NOCASE,
                password_hash     TEXT    NOT NULL,
                fecha_registro    TEXT    NOT NULL,
                activo            INTEGER NOT NULL DEFAULT 1,
                intentos_fallidos INTEGER NOT NULL DEFAULT 0
            );
            """);

        if (configuration.GetValue<bool>("Seed:UsuarioBloqueado"))
            SeedUsuarioBloqueado(connection);

        logger.LogInformation("SQLite inicializado correctamente → {ConnectionString}", connectionString);
    }

    /// <summary>
    /// Usuario con Activo = false e IntentosFallidos = 0: representa un bloqueo manual
    /// (por fraude), así que el login devuelve USR-005.
    /// </summary>
    private void SeedUsuarioBloqueado(SqliteConnection connection)
    {
        var existe = connection.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM users WHERE email = @email", new { email = UsuarioBloqueadoEmail });
        if (existe > 0) return;

        var user = new User { Id = Guid.NewGuid(), Nombre = "Usuario", Apellido = "Bloqueado", Email = UsuarioBloqueadoEmail };
        var hash = passwordHasher.HashPassword(user, UsuarioBloqueadoPassword);

        connection.Execute("""
            INSERT INTO users (id, nombre, apellido, email, password_hash, fecha_registro, activo, intentos_fallidos)
            VALUES (@Id, @Nombre, @Apellido, @Email, @Hash, @Fecha, 0, 0);
            """,
            new
            {
                Id = user.Id.ToString(),
                user.Nombre,
                user.Apellido,
                user.Email,
                Hash = hash,
                Fecha = DateTime.UtcNow.ToString("o")
            });

        logger.LogInformation("Usuario de prueba bloqueado creado: {Email}", UsuarioBloqueadoEmail);
    }
}
