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
public class CartService : ICartService
{
    private readonly ICartRepository _repository;
    private readonly IProductsClient _productsClient;
    private readonly ILogger<CartService> _logger;

    public CartService(ICartRepository repository, IProductsClient productsClient, ILogger<CartService> logger)
    {
        _repository = repository;
        _productsClient = productsClient;
        _logger = logger;
    }

    public async Task<CartResponse> GetAsync(string userId)
    {
        ShoppingCart cart = await GetExistingCartAsync(userId);
        return ToResponse(cart);
    }

    public async Task<CartResponse> AddItemAsync(string userId, AddCartItemRequest request)
    {
        // POST: el único código 400 del catálogo es CRT-004, se usa para todo dato inválido
        Guid usuarioId;
        if (!Guid.TryParse(userId, out usuarioId))
        {
            throw new ValidationException(ErrorCodes.CRT_004, "El userId '" + userId + "' no tiene un formato válido.");
        }

        // ProductoId y Cantidad ya vienen validados como obligatorios por [Required]
        int cantidadPedida = request.Cantidad.Value;
        Guid productoId = request.ProductoId.Value;

        ValidarCantidad(cantidadPedida);

        ProductDto product = await GetProductOrThrowAsync(productoId);

        // Si el producto ya estaba en el carrito, se suma la cantidad nueva
        int cantidadActual = 0;
        ShoppingCart cart = await _repository.GetAsync(usuarioId);
        if (cart != null)
        {
            CartItem itemExistente = BuscarItem(cart, productoId);
            if (itemExistente != null)
            {
                cantidadActual = itemExistente.Cantidad;
            }
        }
        int cantidadTotal = cantidadActual + cantidadPedida;

        ValidarStock(product, cantidadTotal);

        await _repository.SaveItemAsync(usuarioId, productoId, cantidadTotal, DateTime.UtcNow);
        _logger.LogInformation("Producto {ProductoId} agregado al carrito de {UsuarioId}. Cantidad total: {Cantidad}",
            productoId, usuarioId, cantidadTotal);

        ShoppingCart actualizado = await _repository.GetAsync(usuarioId);
        return ToResponse(actualizado);
    }

    public async Task<CartResponse> UpdateItemAsync(string userId, string productId, UpdateCartItemRequest request)
    {
        int cantidadNueva = request.Cantidad.Value;
        ValidarCantidad(cantidadNueva);

        ShoppingCart cart = await GetExistingCartAsync(userId);
        Guid productoId = GetItemIdOrThrow(cart, productId);

        // El producto podría haberse borrado del catálogo o haber cambiado su stock
        ProductDto product = await GetProductOrThrowAsync(productoId);
        ValidarStock(product, cantidadNueva);

        await _repository.SaveItemAsync(cart.UsuarioId, productoId, cantidadNueva, DateTime.UtcNow);
        _logger.LogInformation("Cantidad del producto {ProductoId} actualizada a {Cantidad} en el carrito de {UsuarioId}",
            productoId, cantidadNueva, cart.UsuarioId);

        ShoppingCart actualizado = await _repository.GetAsync(cart.UsuarioId);
        return ToResponse(actualizado);
    }

    public async Task RemoveItemAsync(string userId, string productId)
    {
        ShoppingCart cart = await GetExistingCartAsync(userId);
        Guid productoId = GetItemIdOrThrow(cart, productId);

        await _repository.RemoveItemAsync(cart.UsuarioId, productoId, DateTime.UtcNow);
        _logger.LogInformation("Producto {ProductoId} quitado del carrito de {UsuarioId}", productoId, cart.UsuarioId);
    }

    public async Task ClearAsync(string userId)
    {
        ShoppingCart cart = await GetExistingCartAsync(userId);

        await _repository.ClearAsync(cart.UsuarioId, DateTime.UtcNow);
        _logger.LogInformation("Carrito de {UsuarioId} vaciado", cart.UsuarioId);
    }

    // ── Validaciones ──────────────────────────────────────────────────────────

    /// <summary>CRT-004: la cantidad debe ser mayor a cero.</summary>
    private static void ValidarCantidad(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ValidationException(ErrorCodes.CRT_004,
                "Cantidad inválida. Debe ser mayor a cero (recibido: " + cantidad + ").");
        }
    }

    /// <summary>CRT-003: la cantidad total no puede superar el stock disponible.</summary>
    private static void ValidarStock(ProductDto product, int cantidadSolicitada)
    {
        if (cantidadSolicitada > product.Stock)
        {
            throw new BusinessRuleException(ErrorCodes.CRT_003,
                "Stock insuficiente. Disponible: " + product.Stock + ", solicitado: " + cantidadSolicitada + ".");
        }
    }

    /// <summary>CRT-002: el producto tiene que existir en Products.API.</summary>
    private async Task<ProductDto> GetProductOrThrowAsync(Guid productoId)
    {
        ProductDto product = await _productsClient.GetByIdAsync(productoId);
        if (product == null)
        {
            throw new NotFoundException(ErrorCodes.CRT_002, "Producto no encontrado: '" + productoId + "'.");
        }
        return product;
    }

    /// <summary>CRT-001: el usuario tiene que tener un carrito.</summary>
    private async Task<ShoppingCart> GetExistingCartAsync(string userId)
    {
        Guid usuarioId;
        if (!Guid.TryParse(userId, out usuarioId))
        {
            throw new NotFoundException(ErrorCodes.CRT_001, ErrorMessages.CRT_001);
        }

        ShoppingCart cart = await _repository.GetAsync(usuarioId);
        if (cart == null)
        {
            throw new NotFoundException(ErrorCodes.CRT_001, ErrorMessages.CRT_001);
        }
        return cart;
    }

    /// <summary>CRT-001: el producto tiene que estar dentro del carrito.</summary>
    private static Guid GetItemIdOrThrow(ShoppingCart cart, string productId)
    {
        Guid productoId;
        if (Guid.TryParse(productId, out productoId))
        {
            CartItem item = BuscarItem(cart, productoId);
            if (item != null)
            {
                return productoId;
            }
        }

        throw new NotFoundException(ErrorCodes.CRT_001,
            "Carrito no encontrado: el producto '" + productId + "' no está en el carrito.");
    }

    /// <summary>Recorre los items del carrito y devuelve el de ese producto, o null si no está.</summary>
    private static CartItem BuscarItem(ShoppingCart cart, Guid productoId)
    {
        foreach (CartItem item in cart.Items)
        {
            if (item.ProductoId == productoId)
            {
                return item;
            }
        }
        return null;
    }

    /// <summary>Convierte la entidad ShoppingCart al DTO de respuesta.</summary>
    private static CartResponse ToResponse(ShoppingCart cart)
    {
        var response = new CartResponse();
        response.UsuarioId = cart.UsuarioId;
        response.FechaActualizacion = cart.FechaActualizacion;

        foreach (CartItem item in cart.Items)
        {
            var itemResponse = new CartItemResponse();
            itemResponse.ProductoId = item.ProductoId;
            itemResponse.Cantidad = item.Cantidad;
            response.Items.Add(itemResponse);
        }

        return response;
    }
}
