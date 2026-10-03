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
            .WithTags("Products");

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
            .WithTags("Products");

        app.MapPost("/api/products",
            async (CreateProductRequest request, ItemRepository repo) =>
            {
                if (string.IsNullOrWhiteSpace(request.Nombre) ||
                    string.IsNullOrWhiteSpace(request.Categoria) ||
                    request.Nombre.Length > 100 ||
                    request.Precio <= 0 ||
                    request.Stock < 0)
                {
                    return Results.BadRequest(new
                    {
                        Codigo = "PRD002",
                        Mensaje = "Los datos del producto no son válidos"
                    });
                }

                var product = await repo.CreateAsync(request);

                return Results.Created(
                    $"/api/products/{product.Id}",
                    product);
            })
            .WithTags("Products");

        app.MapPut("/api/products/{id:guid}",
            async (
                Guid id,
                UpdateProductRequest request,
                ItemRepository repo) =>
            {
                if (string.IsNullOrWhiteSpace(request.Nombre) ||
                    string.IsNullOrWhiteSpace(request.Categoria) ||
                    request.Nombre.Length > 100 ||
                    request.Precio <= 0 ||
                    request.Stock < 0)
                {
                    return Results.BadRequest(new
                    {
                        Codigo = "PRD002",
                        Mensaje = "Los datos del producto no son válidos"
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

                await repo.UpdateAsync(id, request);

                var updated = await repo.GetByIdAsync(id);

                return Results.Ok(updated);
            })
            .WithTags("Products");

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
            .WithTags("Products");
    }
}