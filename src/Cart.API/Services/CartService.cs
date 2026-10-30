using Cart.API.Clients;
using Cart.API.Constants;
using Cart.API.Data;
using Cart.API.DTOs;
using Cart.API.Exceptions;
using Cart.API.Models;

namespace Cart.API.Services;

/// <summary>
/// Lógica de negocio del carrito.
/// Para agregar o modificar productos consulta a Products.API (existencia y stock).
/// Cuando algo no se cumple lanza una excepción de dominio con el código del catálogo;
/// el IExceptionHandler correspondiente arma la respuesta HTTP.
/// </summary>
/// <remarks>
/// Los ids de la ruta llegan como texto: si no son un Guid válido se trata como "no encontrado"
/// (o como dato inválido en el POST), así nunca se devuelve un status que la consigna no permite.
/// </remarks>
public class CartService(
    ICartRepository repository,
    IProductsClient productsClient,
    ILogger<CartService> logger) : ICartService
{
    public async Task<CartResponse> GetAsync(string userId)
    {
        var cart = await GetExistingCartAsync(userId);
        return ToResponse(cart);
    }

    public async Task<CartResponse> AddItemAsync(string userId, AddCartItemRequest request)
    {
        // POST: el único código 400 del catálogo es CRT-004, se usa para todo dato inválido
        if (!Guid.TryParse(userId, out var usuarioId))
            throw new ValidationException(ErrorCodes.CRT_004, $"El userId '{userId}' no tiene un formato válido.");

        ValidarCantidad(request.Cantidad!.Value);
        var productoId = request.ProductoId!.Value;

        var product = await GetProductOrThrowAsync(productoId);

        // Si el producto ya estaba en el carrito, se suma la cantidad nueva
        var cart = await repository.GetAsync(usuarioId);
        var cantidadActual = cart?.Items.FirstOrDefault(i => i.ProductoId == productoId)?.Cantidad ?? 0;
        var cantidadTotal = cantidadActual + request.Cantidad.Value;

        ValidarStock(product, cantidadTotal);

        await repository.SaveItemAsync(usuarioId, productoId, cantidadTotal, DateTime.UtcNow);
        logger.LogInformation("Producto {ProductoId} agregado al carrito de {UsuarioId}. Cantidad total: {Cantidad}",
            productoId, usuarioId, cantidadTotal);

        return ToResponse((await repository.GetAsync(usuarioId))!);
    }

    public async Task<CartResponse> UpdateItemAsync(string userId, string productId, UpdateCartItemRequest request)
    {
        ValidarCantidad(request.Cantidad!.Value);

        var cart = await GetExistingCartAsync(userId);
        var productoId = GetItemIdOrThrow(cart, productId);

        // El producto podría haberse borrado del catálogo o haber cambiado su stock
        var product = await GetProductOrThrowAsync(productoId);
        ValidarStock(product, request.Cantidad.Value);

        await repository.SaveItemAsync(cart.UsuarioId, productoId, request.Cantidad.Value, DateTime.UtcNow);
        logger.LogInformation("Cantidad del producto {ProductoId} actualizada a {Cantidad} en el carrito de {UsuarioId}",
            productoId, request.Cantidad.Value, cart.UsuarioId);

        return ToResponse((await repository.GetAsync(cart.UsuarioId))!);
    }

    public async Task RemoveItemAsync(string userId, string productId)
    {
        var cart = await GetExistingCartAsync(userId);
        var productoId = GetItemIdOrThrow(cart, productId);

        await repository.RemoveItemAsync(cart.UsuarioId, productoId, DateTime.UtcNow);
        logger.LogInformation("Producto {ProductoId} quitado del carrito de {UsuarioId}", productoId, cart.UsuarioId);
    }

    public async Task ClearAsync(string userId)
    {
        var cart = await GetExistingCartAsync(userId);

        await repository.ClearAsync(cart.UsuarioId, DateTime.UtcNow);
        logger.LogInformation("Carrito de {UsuarioId} vaciado", cart.UsuarioId);
    }

    // ── Validaciones ──────────────────────────────────────────────────────────

    /// <summary>CRT-004: la cantidad debe ser mayor a cero.</summary>
    private static void ValidarCantidad(int cantidad)
    {
        if (cantidad <= 0)
            throw new ValidationException(ErrorCodes.CRT_004,
                $"Cantidad inválida. Debe ser mayor a cero (recibido: {cantidad}).");
    }

    /// <summary>CRT-003: la cantidad total no puede superar el stock disponible.</summary>
    private static void ValidarStock(ProductDto product, int cantidadSolicitada)
    {
        if (cantidadSolicitada > product.Stock)
            throw new BusinessRuleException(ErrorCodes.CRT_003,
                $"Stock insuficiente. Disponible: {product.Stock}, solicitado: {cantidadSolicitada}.");
    }

    /// <summary>CRT-002: el producto tiene que existir en Products.API.</summary>
    private async Task<ProductDto> GetProductOrThrowAsync(Guid productoId) =>
        await productsClient.GetByIdAsync(productoId)
        ?? throw new NotFoundException(ErrorCodes.CRT_002, $"Producto no encontrado: '{productoId}'.");

    /// <summary>CRT-001: el usuario tiene que tener un carrito.</summary>
    private async Task<ShoppingCart> GetExistingCartAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var usuarioId))
            throw new NotFoundException(ErrorCodes.CRT_001, ErrorMessages.CRT_001);

        return await repository.GetAsync(usuarioId)
               ?? throw new NotFoundException(ErrorCodes.CRT_001, ErrorMessages.CRT_001);
    }

    /// <summary>CRT-001: el producto tiene que estar dentro del carrito.</summary>
    private static Guid GetItemIdOrThrow(ShoppingCart cart, string productId)
    {
        if (Guid.TryParse(productId, out var productoId) && cart.Items.Any(i => i.ProductoId == productoId))
            return productoId;

        throw new NotFoundException(ErrorCodes.CRT_001,
            $"Carrito no encontrado: el producto '{productId}' no está en el carrito.");
    }

    private static CartResponse ToResponse(ShoppingCart cart) => new()
    {
        UsuarioId = cart.UsuarioId,
        FechaActualizacion = cart.FechaActualizacion,
        Items = cart.Items.Select(i => new CartItemResponse { ProductoId = i.ProductoId, Cantidad = i.Cantidad }).ToList()
    };
}
