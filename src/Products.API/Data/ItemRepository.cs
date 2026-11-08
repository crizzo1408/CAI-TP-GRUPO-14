using Dapper;
using Microsoft.Data.Sqlite;
using MiniApi.Models;
using System.Globalization;

namespace MiniApi.Data;

public class ItemRepository
{
    private readonly IConfiguration _config;

    public ItemRepository(IConfiguration config)
    {
        _config = config;
    }

    private SqliteConnection CreateConnection()
    {
        var connectionString =
            _config.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Data Source=products.db";
        }

        return new SqliteConnection(connectionString);
    }

    private static Product ToProduct(ProductDbRow row)
    {
        var product = new Product
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

        return product;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(
        string? categoria = null,
        string? nombre = null)
    {
        string? categoriaFilter = null;
        string? nombreFilter = null;

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            categoriaFilter = categoria.Trim();
        }

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            nombreFilter = nombre.Trim();
        }

        using var connection = CreateConnection();

        var rows = await connection.QueryAsync<ProductDbRow>("""
            SELECT
                id AS Id,
                nombre AS Nombre,
                descripcion AS Descripcion,
                precio AS Precio,
                stock AS Stock,
                categoria AS Categoria,
                fecha_creacion AS FechaCreacion
            FROM products
            WHERE (
                @Categoria IS NULL
                OR LOWER(categoria) = LOWER(@Categoria)
            )
            AND (
                @Nombre IS NULL
                OR LOWER(nombre) LIKE '%' || LOWER(@Nombre) || '%'
            )
            ORDER BY fecha_creacion DESC
        """, new
        {
            Categoria = categoriaFilter,
            Nombre = nombreFilter
        });

        var products = new List<Product>();

        foreach (var row in rows)
        {
            var product = ToProduct(row);
            products.Add(product);
        }

        return products;
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        using var connection = CreateConnection();

        var row =
            await connection.QuerySingleOrDefaultAsync<ProductDbRow>("""
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

        if (row == null)
        {
            return null;
        }

        return ToProduct(row);
    }

    public async Task<bool> ExistsByNameAndCategoryAsync(
        string nombre,
        string categoria)
    {
        using var connection = CreateConnection();

        var normalizedName = nombre.Trim();
        var normalizedCategory = categoria.Trim();

        var count = await connection.ExecuteScalarAsync<int>("""
            SELECT COUNT(1)
            FROM products
            WHERE LOWER(TRIM(nombre)) = LOWER(@Nombre)
              AND LOWER(TRIM(categoria)) = LOWER(@Categoria)
        """, new
        {
            Nombre = normalizedName,
            Categoria = normalizedCategory
        });

        if (count > 0)
        {
            return true;
        }

        return false;
    }

    public async Task<Product> CreateAsync(
        CreateProductRequest request)
    {
        using var connection = CreateConnection();

        var normalizedDescription =
            NormalizeOptionalText(request.Descripcion);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre.Trim(),
            Descripcion = normalizedDescription,
            Precio = request.Precio,
            Stock = request.Stock,
            Categoria = request.Categoria.Trim(),
            FechaCreacion = DateTime.UtcNow
        };

        await connection.ExecuteAsync("""
            INSERT INTO products
                (
                    id,
                    nombre,
                    descripcion,
                    precio,
                    stock,
                    categoria,
                    fecha_creacion
                )
            VALUES
                (
                    @Id,
                    @Nombre,
                    @Descripcion,
                    @Precio,
                    @Stock,
                    @Categoria,
                    @FechaCreacion
                )
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
        using var connection = CreateConnection();

        var normalizedDescription =
            NormalizeOptionalText(request.Descripcion);

        var rows = await connection.ExecuteAsync("""
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
            Nombre = request.Nombre.Trim(),
            Descripcion = normalizedDescription,
            request.Precio,
            request.Stock,
            Categoria = request.Categoria.Trim()
        });

        if (rows > 0)
        {
            return true;
        }

        return false;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = CreateConnection();

        var rows = await connection.ExecuteAsync(
            "DELETE FROM products WHERE id = @Id",
            new
            {
                Id = id.ToString()
            });

        if (rows > 0)
        {
            return true;
        }

        return false;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (value == null)
        {
            return null;
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length == 0)
        {
            return null;
        }

        return normalizedValue;
    }

    private sealed class ProductDbRow
    {
        public string Id { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public double Precio { get; set; }

        public long Stock { get; set; }

        public string Categoria { get; set; } = string.Empty;

        public string FechaCreacion { get; set; } = string.Empty;
    }
}