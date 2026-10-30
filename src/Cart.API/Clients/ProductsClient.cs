using System.Net;
using Cart.API.Constants;
using Cart.API.Exceptions;

namespace Cart.API.Clients;

/// <summary>
/// Cliente HTTP para Products.API (se crea con IHttpClientFactory).
/// Siempre verifica el status HTTP antes de leer el body (recomendación de la consigna).
/// El header X-Correlation-Id lo agrega CorrelationIdDelegatingHandler.
/// </summary>
public class ProductsClient(HttpClient httpClient, ILogger<ProductsClient> logger) : IProductsClient
{
    public async Task<ProductDto?> GetByIdAsync(Guid productoId, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync($"api/products/{productoId}", cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Products caído, puerto equivocado o timeout
            logger.LogError(ex, "No se pudo comunicar con Products.API al buscar el producto {ProductoId}", productoId);
            throw new ExternalServiceException(ErrorCodes.CRT_005,
                "No se pudo comunicar con Products.API para validar el producto.");
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogInformation("Products.API respondió 404 para el producto {ProductoId}", productoId);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Products.API respondió {StatusCode} al buscar el producto {ProductoId}",
                    (int)response.StatusCode, productoId);
                throw new ExternalServiceException(ErrorCodes.CRT_005,
                    $"Products.API respondió con un error ({(int)response.StatusCode}) al validar el producto.");
            }

            var product = await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);
            return product ?? throw new ExternalServiceException(ErrorCodes.CRT_005,
                "Products.API devolvió una respuesta vacía.");
        }
    }
}
