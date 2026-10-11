using MiniApi.Data;
using MiniApi.Models;

namespace MiniApi.Extensions.Endpoints;

public static class ItemsEndpointsExtension
{
    public static void MapItemEndpoints(this WebApplication app)
    {
        app.MapGet("/api/products",
            async (
                string? categoria,
                string? nombre,
                ItemRepository repo) =>
            {
                var products = await repo.GetAllAsync(categoria, nombre);
                return Results.Ok(products);
            })
            .WithTags("Products")
            .Produces<IEnumerable<Product>>(StatusCodes.Status200OK);

        app.MapGet("/api/products/{id:guid}",
            async (Guid id, ItemRepository repo) =>
            {
                var product = await repo.GetByIdAsync(id);

                return product is null
                    ? Results.NotFound(new
                    {
                        Codigo = "PRD001",
                        Mensaje = "Producto no encontrado"
                    })
                    : Results.Ok(product);
            })
            .WithTags("Products")
            .Produces<Product>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/api/products",
            async (CreateProductRequest request, ItemRepository repo) =>
            {
                var errors = ValidateProduct(
                    request.Nombre,
                    request.Descripcion,
                    request.Precio,
                    request.Stock,
                    request.Categoria);

                if (errors.Count > 0)
                {
                    return Results.BadRequest(new
                    {
                        Codigo = "PRD002",
                        Mensaje = "Los datos del producto no son válidos",
                        Errores = errors
                    });
                }

                if (await repo.ExistsByNameAndCategoryAsync(
                    request.Nombre,
                    request.Categoria))
                {
                    return Results.Conflict(new
                    {
                        Codigo = "PRD003",
                        Mensaje = "Ya existe un producto con el mismo nombre y categoría"
                    });
                }

                var product = await repo.CreateAsync(request);

                return Results.Created(
                    $"/api/products/{product.Id}",
                    product);
            })
            .WithTags("Products")
            .Produces<Product>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);

        app.MapPut("/api/products/{id:guid}",
            async (
                Guid id,
                UpdateProductRequest request,
                ItemRepository repo) =>
            {
                var errors = ValidateProduct(
                    request.Nombre,
                    request.Descripcion,
                    request.Precio,
                    request.Stock,
                    request.Categoria);

                if (errors.Count > 0)
                {
                    return Results.BadRequest(new
                    {
                        Codigo = "PRD002",
                        Mensaje = "Los datos del producto no son válidos",
                        Errores = errors
                    });
                }

                var exists = await repo.GetByIdAsync(id);

                if (exists is null)
                {
                    return Results.NotFound(new
                    {
                        Codigo = "PRD001",
                        Mensaje = "Producto no encontrado"
                    });
                }

                if (await repo.ExistsByNameAndCategoryAsync(
                    request.Nombre,
                    request.Categoria,
                    id))
                {
                    return Results.Conflict(new
                    {
                        Codigo = "PRD003",
                        Mensaje = "Ya existe un producto con el mismo nombre y categoría"
                    });
                }

                await repo.UpdateAsync(id, request);

                var updated = await repo.GetByIdAsync(id);

                return Results.Ok(updated);
            })
            .WithTags("Products")
            .Produces<Product>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        app.MapDelete("/api/products/{id:guid}",
    async (Guid id, ItemRepository repo) =>
    {
        var exists = await repo.GetByIdAsync(id);

        if (exists is null)
        {
            return Results.NotFound(new
            {
                Codigo = "PRD001",
                Mensaje = "Producto no encontrado"
            });
        }

        await repo.DeleteAsync(id);

        return Results.NoContent();
    })
    .WithTags("Products")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status409Conflict);

    }

    private static List<string> ValidateProduct(
        string nombre,
        string? descripcion,
        decimal precio,
        int stock,
        string categoria)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errors.Add("El nombre es obligatorio.");
        }
        else if (nombre.Length > 100)
        {
            errors.Add("El nombre no puede superar los 100 caracteres.");
        }

        if (descripcion?.Length > 500)
        {
            errors.Add("La descripción no puede superar los 500 caracteres.");
        }

        if (precio <= 0)
        {
            errors.Add("El precio debe ser mayor a 0.");
        }

        if (stock < 0)
        {
            errors.Add("El stock no puede ser negativo.");
        }

        if (string.IsNullOrWhiteSpace(categoria))
        {
            errors.Add("La categoría es obligatoria.");
        }

        return errors;
    }
}