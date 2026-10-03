using Dapper;
using Microsoft.Data.Sqlite;

namespace MiniApi.Data;

public class DatabaseInitializer
{
    private readonly IConfiguration _config;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        IConfiguration config,
        ILogger<DatabaseInitializer> logger)
    {
        _config = config;
        _logger = logger;
    }

    public void Initialize()
    {
        var connectionString =
            _config.GetConnectionString("DefaultConnection")
            ?? "Data Source=products.db";

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        connection.Execute("""
            CREATE TABLE IF NOT EXISTS products (
                id              TEXT PRIMARY KEY,
                nombre          TEXT NOT NULL,
                descripcion     TEXT,
                precio           REAL NOT NULL CHECK (precio > 0),
                stock            INTEGER NOT NULL CHECK (stock >= 0),
                categoria       TEXT NOT NULL,
                fecha_creacion  TEXT NOT NULL
            );
        """);

        _logger.LogInformation(
            "SQLite inicializado correctamente → {db}",
            connectionString);
    }
}