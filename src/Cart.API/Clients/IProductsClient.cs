namespace Cart.API.Clients;

public interface IProductsClient
{
    /// <summary>
    /// Busca un producto en Products.API.
    /// Devuelve null si no existe (404). Lanza ExternalServiceException si Products falla o no responde.
    /// </summary>
    Task<ProductDto?> GetByIdAsync(Guid productoId, CancellationToken cancellationToken = default);
}
