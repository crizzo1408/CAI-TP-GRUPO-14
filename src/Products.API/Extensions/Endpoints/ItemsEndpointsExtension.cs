using MiniApi.Clients;
using MiniApi.Data;
using MiniApi.Exceptions;
using MiniApi.Models;

namespace MiniApi.Extensions.Endpoints;

public static class ItemsEndpointsExtension
{
    public static void MapItemEndpoints(
     this WebApplication app)
    {
        app.MapGet(
                "/api/products",
                GetAllProductsAsync)
            .WithName("GetProducts")
            .WithTags("Products")
            .WithSummary("Lista los productos")
            .WithDescription(
                "Permite filtrar por categoria y nombre.")
            .Produces<IEnumerable<Product>>(
                StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status500InternalServerError);

        app.MapGet(
                "/api/products/{id:guid}",
                GetProductByIdAsync)
            .WithName("GetProductById")
            .WithTags("Products")
            .WithSummary("Obtiene un producto por ID")
            .WithDescription(
                "Devuelve PRD-001 cuando el producto no existe.")
            .Produces<Product>(
                StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status500InternalServerError);

        app.MapPost(
                "/api/products",
                CreateProductAsync)
            .WithName("CreateProduct")
            .WithTags("Products")
            .WithSummary("Crea un producto")
            .WithDescription(
                "Valida los datos y evita productos duplicados.")
            .Produces<Product>(
                StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status409Conflict)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status500InternalServerError);

        app.MapPut(
                "/api/products/{id:guid}",
                UpdateProductAsync)
            .WithName("UpdateProduct")
            .WithTags("Products")
            .WithSummary("Actualiza un producto")
            .WithDescription(
                "Actualiza los datos del producto indicado.")
            .Produces<Product>(
                StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status500InternalServerError);

        app.MapDelete(
                "/api/products/{id:guid}",
                DeleteProductAsync)
            .WithName("DeleteProduct")
            .WithTags("Products")
            .WithSummary("Elimina un producto")
            .WithDescription(
                "Impide eliminar productos con órdenes activas.")
            .Produces(
                StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status409Conflict)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetAllProductsAsync(
        string? categoria,
        string? nombre,
        ItemRepository repository)
    {
        var products = await repository.GetAllAsync(
            categoria,
            nombre);

        return Results.Ok(products);
    }

    private static async Task<IResult> GetProductByIdAsync(
        Guid id,
        ItemRepository repository)
    {
        var product = await repository.GetByIdAsync(id);

        if (product == null)
        {
            throw new NotFoundException(
                ErrorCodes.ProductNotFound,
                "Producto no encontrado.");
        }

        return Results.Ok(product);
    }

    private static async Task<IResult> CreateProductAsync(
        CreateProductRequest request,
        ItemRepository repository)
    {
        var errors = ValidateProduct(
            request.Nombre,
            request.Descripcion,
            request.Precio,
            request.Stock,
            request.Categoria);

        if (errors.Count > 0)
        {
            throw new ValidationException(
                ErrorCodes.InvalidProduct,
                errors);
        }

        var productExists =
            await repository.ExistsByNameAndCategoryAsync(
                request.Nombre,
                request.Categoria);

        if (productExists)
        {
            throw new BusinessRuleException(
                ErrorCodes.DuplicateProduct,
                "Ya existe un producto con ese nombre " +
                "en la categoría.");
        }

        var product = await repository.CreateAsync(request);

        return Results.Created(
            "/api/products/" + product.Id,
            product);
    }

    private static async Task<IResult> UpdateProductAsync(
        Guid id,
        UpdateProductRequest request,
        ItemRepository repository)
    {
        var errors = ValidateProduct(
            request.Nombre,
            request.Descripcion,
            request.Precio,
            request.Stock,
            request.Categoria);

        if (errors.Count > 0)
        {
            throw new ValidationException(
                ErrorCodes.InvalidProduct,
                errors);
        }

        var existingProduct =
            await repository.GetByIdAsync(id);

        if (existingProduct == null)
        {
            throw new NotFoundException(
                ErrorCodes.ProductNotFound,
                "Producto no encontrado.");
        }

        await repository.UpdateAsync(id, request);

        var updatedProduct =
            await repository.GetByIdAsync(id);

        return Results.Ok(updatedProduct);
    }

    private static async Task<IResult> DeleteProductAsync(
    Guid id,
    ItemRepository repository,
    OrdersClient ordersClient,
    CancellationToken cancellationToken)
    {
        var existingProduct =
            await repository.GetByIdAsync(id);

        if (existingProduct == null)
        {
            throw new NotFoundException(
                ErrorCodes.ProductNotFound,
                "Producto no encontrado.");
        }

        var hasActiveOrders =
            await ordersClient.HasActiveOrdersAsync(
                id,
                cancellationToken);

        if (hasActiveOrders)
        {
            throw new BusinessRuleException(
                ErrorCodes.ProductWithActiveOrders,
                "El producto tiene órdenes activas " +
                "y no puede eliminarse.");
        }

        await repository.DeleteAsync(id);

        return Results.NoContent();
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
        else
        {
            if (nombre.Length > 100)
            {
                errors.Add(
                    "El nombre no puede superar los 100 caracteres.");
            }
        }

        if (descripcion != null)
        {
            if (descripcion.Length > 500)
            {
                errors.Add(
                    "La descripción no puede superar los 500 caracteres.");
            }
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