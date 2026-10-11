using Dapper;
using Microsoft.Data.Sqlite;
using MiniApi.Models;
using System.Globalization;

namespace MiniApi.Data;

public class ItemRepository
{
    private readonly IConfiguration _config;

    public ItemRepository(IConfiguration config) => _config = config;

    private SqliteConnection CreateConnection() =>
        new(_config.GetConnectionString("DefaultConnection")
            ?? "Data Source=products.db");

    private static Product ToProduct(ProductDbRow row)
    {
        return new Product
        {
            Id = Guid.Parse(row.Id),
            Nombre = row.Nombre,
            Descripcion = row.Descripcion,
            Precio = (decimal)row.Precio,
            Stock = (int)row.Stock,
            Categoria = row.Categoria,
            FechaCreacion = DateTime.Parse(
                row.FechaCreacion,
                CultureInfo.InvariantCulture)
        };
    }

    public async Task<IEnumerable<Product>> GetAllAsync(
        string? categoria = null,
        string? nombre = null)
    {
        using var conn = CreateConnection();

        var rows = await conn.QueryAsync<ProductDbRow>("""
            SELECT
                id AS Id,
                nombre AS Nombre,
                descripcion AS Descripcion,
                precio AS Precio,
                stock AS Stock,
                categoria AS Categoria,
                fecha_creacion AS FechaCreacion
            FROM products
            WHERE (@Categoria IS NULL OR categoria = @Categoria)
              AND (@Nombre IS NULL OR nombre LIKE '%' || @Nombre || '%')
            ORDER BY fecha_creacion DESC
        """, new
        {
            Categoria = categoria,
            Nombre = nombre
        });

        return rows.Select(ToProduct);
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        using var conn = CreateConnection();

        var row = await conn.QuerySingleOrDefaultAsync<ProductDbRow>("""
            SELECT
                id AS Id,
                nombre AS Nombre,
                descripcion AS Descripcion,
                precio AS Precio,
                stock AS Stock,
                categoria AS Categoria,
                fecha_creacion AS FechaCreacion
            FROM products
            WHERE id = @Id
        """, new
        {
            Id = id.ToString()
        });

        return row is null ? null : ToProduct(row);
    }
    public async Task<bool> ExistsByNameAndCategoryAsync(
    string nombre,
    string categoria,
    Guid? excludeId = null)
    {
        using var conn = CreateConnection();

        var count = await conn.ExecuteScalarAsync<int>("""
            SELECT COUNT(1)
            FROM products
            WHERE LOWER(nombre) = LOWER(@Nombre)
              AND LOWER(categoria) = LOWER(@Categoria)
              AND (@ExcludeId IS NULL OR id <> @ExcludeId)
        """, new
        {
            Nombre = nombre.Trim(),
            Categoria = categoria.Trim(),
            ExcludeId = excludeId?.ToString()
        });

        return count > 0;
    }

    public async Task<Product> CreateAsync(CreateProductRequest request)
    {
        using var conn = CreateConnection();

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            Stock = request.Stock,
            Categoria = request.Categoria,
            FechaCreacion = DateTime.UtcNow
        };

        await conn.ExecuteAsync("""
            INSERT INTO products
                (id, nombre, descripcion, precio, stock, categoria, fecha_creacion)
            VALUES
                (@Id, @Nombre, @Descripcion, @Precio, @Stock, @Categoria, @FechaCreacion)
        """, new
        {
            Id = product.Id.ToString(),
            product.Nombre,
            product.Descripcion,
            product.Precio,
            product.Stock,
            product.Categoria,
            product.FechaCreacion
        });

        return product;
    }

    public async Task<bool> UpdateAsync(
        Guid id,
        UpdateProductRequest request)
    {
        using var conn = CreateConnection();

        var rows = await conn.ExecuteAsync("""
            UPDATE products
            SET nombre = @Nombre,
                descripcion = @Descripcion,
                precio = @Precio,
                stock = @Stock,
                categoria = @Categoria
            WHERE id = @Id
        """, new
        {
            Id = id.ToString(),
            request.Nombre,
            request.Descripcion,
            request.Precio,
            request.Stock,
            request.Categoria
        });

        return rows > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var conn = CreateConnection();

        var rows = await conn.ExecuteAsync(
            "DELETE FROM products WHERE id = @Id",
            new { Id = id.ToString() });

        return rows > 0;
    }

    private sealed class ProductDbRow
    {
        public string Id { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public double Precio { get; set; }
        public long Stock { get; set; }
        public string Categoria { get; set; } = "";
        public string FechaCreacion { get; set; } = "";
    }
}