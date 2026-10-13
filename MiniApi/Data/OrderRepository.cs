using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using MiniApi.Models;

namespace MiniApi.Data
{
    public class OrderRepository
    {
        private readonly IConfiguration _config;

        public OrderRepository(IConfiguration config) => _config = config;

        private SqliteConnection CreateConnection() =>
            new(_config.GetConnectionString("DefaultConnection") ?? "Data Source=app.db");

        // Filas tal cual salen de SQLite (Guid y fechas se guardan como TEXT)
        private record OrderRow
        {
            public string Id { get; init; } = string.Empty;
            public string UsuarioId { get; init; } = string.Empty;
            public double Total { get; init; }
            public string Estado { get; init; } = string.Empty;
            public string FechaCreacion { get; init; } = string.Empty;
        }

        private record OrderItemRow
        {
            public string OrderId { get; init; } = string.Empty;
            public string ProductoId { get; init; } = string.Empty;
            public long Cantidad { get; init; }
            public double PrecioUnitario { get; init; }
        }

        private static string Now() =>
            DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

        private static DateTime ParseDate(string s) =>
            DateTime.Parse(s, CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);

        private static Order ToOrder(OrderRow r, IEnumerable<OrderItemRow> items) => new()
        {
            Id = Guid.Parse(r.Id),
            UsuarioId = Guid.Parse(r.UsuarioId),
            Total = decimal.Round((decimal)r.Total, 2),
            Estado = r.Estado,
            FechaCreacion = ParseDate(r.FechaCreacion),
            Items = items.Select(i => new OrderItem
            {
                ProductoId = Guid.Parse(i.ProductoId),
                Cantidad = (int)i.Cantidad,
                PrecioUnitario = decimal.Round((decimal)i.PrecioUnitario, 2)
            }).ToList()
        };

        //GET ALL (filtro opcional por usuario)
        public async Task<IEnumerable<Order>> GetAllAsync(Guid? usuarioId)
        {
            using var conn = CreateConnection();

            var rows = (await conn.QueryAsync<OrderRow>("""
            SELECT id, usuario_id AS UsuarioId, total, estado, fecha_creacion AS FechaCreacion
            FROM orders
            WHERE (@usuarioId IS NULL OR usuario_id = @usuarioId)
            ORDER BY fecha_creacion DESC
        """, new { usuarioId = usuarioId?.ToString() })).ToList();

            if (rows.Count == 0) return new List<Order>();

            var items = await conn.QueryAsync<OrderItemRow>("""
            SELECT order_id AS OrderId, producto_id AS ProductoId, cantidad,
                   precio_unitario AS PrecioUnitario
            FROM order_items
            WHERE order_id IN @ids
        """, new { ids = rows.Select(r => r.Id).ToList() });

            var byOrder = items.ToLookup(i => i.OrderId);
            return rows.Select(r => ToOrder(r, byOrder[r.Id])).ToList();
        }

        //GET BY ID 
        public async Task<Order?> GetByIdAsync(Guid id)
        {
            using var conn = CreateConnection();

            var row = await conn.QuerySingleOrDefaultAsync<OrderRow>("""
            SELECT id, usuario_id AS UsuarioId, total, estado, fecha_creacion AS FechaCreacion
            FROM orders
            WHERE id = @id
        """, new { id = id.ToString() });

            if (row is null) return null;

            var items = await conn.QueryAsync<OrderItemRow>("""
            SELECT order_id AS OrderId, producto_id AS ProductoId, cantidad,
                   precio_unitario AS PrecioUnitario
            FROM order_items
            WHERE order_id = @id
        """, new { id = id.ToString() });

            return ToOrder(row, items);
        }

        //CREATE (orden + items en una sola transacción)
        public async Task<Order> CreateAsync(Guid usuarioId, List<OrderItem> items)
        {
            var id = Guid.NewGuid().ToString();
            var total = items.Sum(i => i.PrecioUnitario * i.Cantidad);

            using var conn = CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            await conn.ExecuteAsync("""
            INSERT INTO orders (id, usuario_id, total, estado, fecha_creacion)
            VALUES (@Id, @UsuarioId, @Total, @Estado, @FechaCreacion)
        """, new
            {
                Id = id,
                UsuarioId = usuarioId.ToString(),
                Total = (double)total,
                Estado = OrderStatus.Pendiente,
                FechaCreacion = Now()
            }, tx);

            await conn.ExecuteAsync("""
            INSERT INTO order_items (order_id, producto_id, cantidad, precio_unitario)
            VALUES (@OrderId, @ProductoId, @Cantidad, @PrecioUnitario)
        """, items.Select(i => new
            {
                OrderId = id,
                ProductoId = i.ProductoId.ToString(),
                i.Cantidad,
                PrecioUnitario = (double)i.PrecioUnitario
            }), tx);

            tx.Commit();

            return (await GetByIdAsync(Guid.Parse(id)))!;
        }

        //UPDATE STATUS
        // Devuelve la fecha de actualización aplicada, o null si la orden no existe
        public async Task<DateTime?> UpdateStatusAsync(Guid id, string estado)
        {
            var now = Now();

            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync("""
            UPDATE orders
            SET estado = @Estado,
                fecha_actualizacion = @Fecha
            WHERE id = @Id
        """, new { Estado = estado, Fecha = now, Id = id.ToString() });

            return rows > 0 ? ParseDate(now) : null;
        }
    }
}

