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

        public OrderRepository(IConfiguration config)
        {
            _config = config;
        }

        private SqliteConnection CreateConnection()
        {
            string? connectionString = _config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Data Source=app.db";
            }
            return new SqliteConnection(connectionString);
        }

     
        public class OrderRow
        {
            public string Id { get; set; } = string.Empty;
            public string UsuarioId { get; set; } = string.Empty;
            public double Total { get; set; }
            public string Estado { get; set; } = string.Empty;
            public string FechaCreacion { get; set; } = string.Empty;
        }

        public class OrderItemRow
        {
            public string ProductoId { get; set; } = string.Empty;
            public long Cantidad { get; set; }
            public double PrecioUnitario { get; set; }
        }

        private static string Now()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
        }

        private static DateTime ParseDate(string s)
        {
            return DateTime.Parse(s, CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        }

        private static Order ToOrder(OrderRow row, List<OrderItem> items)
        {
            return new Order
            {
                Id = Guid.Parse(row.Id),
                UsuarioId = Guid.Parse(row.UsuarioId),
                Total = decimal.Round((decimal)row.Total, 2),
                Estado = row.Estado,
                FechaCreacion = ParseDate(row.FechaCreacion),
                Items = items
            };
        }

        
        private static async Task<List<OrderItem>> GetItemsAsync(SqliteConnection conn, string orderId)
        {
            IEnumerable<OrderItemRow> rows = await conn.QueryAsync<OrderItemRow>("""
                SELECT producto_id AS ProductoId, cantidad AS Cantidad, precio_unitario AS PrecioUnitario
                FROM order_items
                WHERE order_id = @OrderId
            """, new { OrderId = orderId });

            List<OrderItem> items = new List<OrderItem>();
            foreach (OrderItemRow row in rows)
            {
                items.Add(new OrderItem
                {
                    ProductoId = Guid.Parse(row.ProductoId),
                    Cantidad = (int)row.Cantidad,
                    PrecioUnitario = decimal.Round((decimal)row.PrecioUnitario, 2)
                });
            }
            return items;
        }

    
        public async Task<List<Order>> GetAllAsync(Guid? usuarioId)
        {
            string? filtro = null;
            if (usuarioId.HasValue)
            {
                filtro = usuarioId.Value.ToString();
            }

            using SqliteConnection conn = CreateConnection();
            await conn.OpenAsync();

            IEnumerable<OrderRow> rows = await conn.QueryAsync<OrderRow>("""
                SELECT id AS Id, usuario_id AS UsuarioId, total AS Total, estado AS Estado,
                       fecha_creacion AS FechaCreacion
                FROM orders
                WHERE (@UsuarioId IS NULL OR usuario_id = @UsuarioId)
                ORDER BY fecha_creacion DESC
            """, new { UsuarioId = filtro });

            List<Order> orders = new List<Order>();
            foreach (OrderRow row in rows)
            {
                List<OrderItem> items = await GetItemsAsync(conn, row.Id);
                orders.Add(ToOrder(row, items));
            }
            return orders;
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            using SqliteConnection conn = CreateConnection();
            await conn.OpenAsync();

            OrderRow? row = await conn.QuerySingleOrDefaultAsync<OrderRow>("""
                SELECT id AS Id, usuario_id AS UsuarioId, total AS Total, estado AS Estado,
                       fecha_creacion AS FechaCreacion
                FROM orders
                WHERE id = @Id
            """, new { Id = id.ToString() });

            if (row == null)
            {
                return null;
            }

            List<OrderItem> items = await GetItemsAsync(conn, row.Id);
            return ToOrder(row, items);
        }

        // CREATE (orden + items en una sola transacción). Estado inicial: Pendiente.
        public async Task<Order> CreateAsync(Guid usuarioId, List<OrderItem> items)
        {
            Guid id = Guid.NewGuid();
            string fecha = Now();

            
            decimal total = 0;
            foreach (OrderItem item in items)
            {
                total = total + (item.PrecioUnitario * item.Cantidad);
            }

            using SqliteConnection conn = CreateConnection();
            await conn.OpenAsync();
            using SqliteTransaction tx = conn.BeginTransaction();

            await conn.ExecuteAsync("""
                INSERT INTO orders (id, usuario_id, total, estado, fecha_creacion)
                VALUES (@Id, @UsuarioId, @Total, @Estado, @FechaCreacion)
            """, new
            {
                Id = id.ToString(),
                UsuarioId = usuarioId.ToString(),
                Total = (double)total,
                Estado = OrderStatus.Pendiente,
                FechaCreacion = fecha
            }, tx);

            foreach (OrderItem item in items)
            {
                await conn.ExecuteAsync("""
                    INSERT INTO order_items (order_id, producto_id, cantidad, precio_unitario)
                    VALUES (@OrderId, @ProductoId, @Cantidad, @PrecioUnitario)
                """, new
                {
                    OrderId = id.ToString(),
                    ProductoId = item.ProductoId.ToString(),
                    Cantidad = item.Cantidad,
                    PrecioUnitario = (double)item.PrecioUnitario
                }, tx);
            }

            tx.Commit();

            return new Order
            {
                Id = id,
                UsuarioId = usuarioId,
                Items = items,
                Total = decimal.Round(total, 2),
                Estado = OrderStatus.Pendiente,
                FechaCreacion = ParseDate(fecha)
            };
        }

        
        public async Task<DateTime?> UpdateStatusAsync(Guid id, string estado)
        {
            string fecha = Now();

            using SqliteConnection conn = CreateConnection();
            int filas = await conn.ExecuteAsync("""
                UPDATE orders
                SET estado = @Estado,
                    fecha_actualizacion = @Fecha
                WHERE id = @Id
            """, new { Estado = estado, Fecha = fecha, Id = id.ToString() });

            if (filas == 0)
            {
                return null;
            }
            return ParseDate(fecha);
        }
    }
}