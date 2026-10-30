using Dapper;
using Microsoft.Data.Sqlite;

namespace Cart.API.Data;

/// <summary>
/// Crea las tablas del carrito en SQLite si no existen (mismo patrón que la plantilla MiniApi).
/// </summary>
public class DatabaseInitializer(IConfiguration configuration, ILogger<DatabaseInitializer> logger)
{
    public void Initialize()
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        // Un carrito por usuario; sus productos en cart_items. Los Guid y fechas se guardan como TEXT.
        connection.Execute("""
            CREATE TABLE IF NOT EXISTS carts (
                usuario_id          TEXT PRIMARY KEY,
                fecha_actualizacion TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS cart_items (
                usuario_id  TEXT    NOT NULL,
                producto_id TEXT    NOT NULL,
                cantidad    INTEGER NOT NULL CHECK (cantidad > 0),
                PRIMARY KEY (usuario_id, producto_id),
                FOREIGN KEY (usuario_id) REFERENCES carts (usuario_id) ON DELETE CASCADE
            );
            """);

        logger.LogInformation("SQLite inicializado correctamente → {ConnectionString}", connectionString);
    }
}
