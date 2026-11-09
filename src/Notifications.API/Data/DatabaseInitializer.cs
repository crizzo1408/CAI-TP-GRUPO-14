using Dapper;
using Microsoft.Data.Sqlite;

namespace Notifications.API.Data;

public class DatabaseInitializer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IConfiguration configuration, ILogger<DatabaseInitializer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void Initialize()
    {
        string connectionString = _configuration.GetConnectionString("DefaultConnection");
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS notifications (
                id          TEXT PRIMARY KEY,
                usuario_id  TEXT NOT NULL,
                mensaje     TEXT NOT NULL CHECK(length(mensaje) BETWEEN 1 AND 500),
                tipo        TEXT NOT NULL CHECK(tipo IN ('Email', 'Push', 'SMS')),
                estado      TEXT NOT NULL CHECK(estado IN ('Pendiente', 'Enviada', 'Fallida')),
                fecha_envio TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS ix_notifications_usuario_id
            ON notifications(usuario_id);");

        _logger.LogInformation("SQLite inicializado correctamente → {ConnectionString}", connectionString);
    }
}
