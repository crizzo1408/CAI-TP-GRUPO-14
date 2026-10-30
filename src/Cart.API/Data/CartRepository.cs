using System.Globalization;
using Cart.API.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Cart.API.Data;

/// <summary>
/// Implementación con SQLite + Dapper (mismo patrón que ItemRepository de la plantilla).
/// Las operaciones que tocan dos tablas usan una transacción.
/// </summary>
public class CartRepository(IConfiguration configuration) : ICartRepository
{
    private SqliteConnection CreateConnection() =>
        new(configuration.GetConnectionString("DefaultConnection"));

    public async Task<ShoppingCart?> GetAsync(Guid usuarioId)
    {
        using var connection = CreateConnection();
        var id = usuarioId.ToString();

        var fecha = await connection.QuerySingleOrDefaultAsync<string>(
            "SELECT fecha_actualizacion FROM carts WHERE usuario_id = @id", new { id });
        if (fecha is null) return null;

        var items = await connection.QueryAsync<CartItemRow>("""
            SELECT producto_id AS ProductoId, cantidad AS Cantidad
            FROM cart_items
            WHERE usuario_id = @id
            ORDER BY rowid
            """, new { id });

        return new ShoppingCart
        {
            UsuarioId = usuarioId,
            FechaActualizacion = DateTime.Parse(fecha, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            Items = items.Select(i => new CartItem { ProductoId = Guid.Parse(i.ProductoId), Cantidad = (int)i.Cantidad }).ToList()
        };
    }

    public async Task SaveItemAsync(Guid usuarioId, Guid productoId, int cantidad, DateTime fechaActualizacion)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        // Crea el carrito si no existe; si existe, actualiza la fecha
        await connection.ExecuteAsync("""
            INSERT INTO carts (usuario_id, fecha_actualizacion) VALUES (@UsuarioId, @Fecha)
            ON CONFLICT (usuario_id) DO UPDATE SET fecha_actualizacion = excluded.fecha_actualizacion;
            """,
            new { UsuarioId = usuarioId.ToString(), Fecha = fechaActualizacion.ToString("o") }, transaction);

        // Agrega el producto; si ya estaba, reemplaza la cantidad (el service ya calculó el total)
        await connection.ExecuteAsync("""
            INSERT INTO cart_items (usuario_id, producto_id, cantidad) VALUES (@UsuarioId, @ProductoId, @Cantidad)
            ON CONFLICT (usuario_id, producto_id) DO UPDATE SET cantidad = excluded.cantidad;
            """,
            new { UsuarioId = usuarioId.ToString(), ProductoId = productoId.ToString(), Cantidad = cantidad }, transaction);

        transaction.Commit();
    }

    public async Task RemoveItemAsync(Guid usuarioId, Guid productoId, DateTime fechaActualizacion)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(
            "DELETE FROM cart_items WHERE usuario_id = @UsuarioId AND producto_id = @ProductoId",
            new { UsuarioId = usuarioId.ToString(), ProductoId = productoId.ToString() }, transaction);

        await TouchAsync(connection, transaction, usuarioId, fechaActualizacion);
        transaction.Commit();
    }

    public async Task ClearAsync(Guid usuarioId, DateTime fechaActualizacion)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(
            "DELETE FROM cart_items WHERE usuario_id = @UsuarioId",
            new { UsuarioId = usuarioId.ToString() }, transaction);

        await TouchAsync(connection, transaction, usuarioId, fechaActualizacion);
        transaction.Commit();
    }

    /// <summary>Actualiza la fecha de última modificación del carrito.</summary>
    private static Task TouchAsync(SqliteConnection connection, SqliteTransaction transaction, Guid usuarioId, DateTime fecha) =>
        connection.ExecuteAsync(
            "UPDATE carts SET fecha_actualizacion = @Fecha WHERE usuario_id = @UsuarioId",
            new { UsuarioId = usuarioId.ToString(), Fecha = fecha.ToString("o") }, transaction);

    /// <summary>Fila tal como viene de SQLite.</summary>
    private sealed class CartItemRow
    {
        public string ProductoId { get; set; } = string.Empty;
        public long Cantidad { get; set; }
    }
}
