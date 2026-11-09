using System.Net;
using Cart.API.Constants;
using Cart.API.Exceptions;

namespace Cart.API.Clients;

/// <summary>
/// Cliente HTTP para Products.API (se crea con IHttpClientFactory).
/// Siempre verifica el status HTTP antes de leer el body (recomendación de la consigna).
/// El header X-Correlation-Id lo agrega CorrelationIdDelegatingHandler.
/// </summary>
public class ProductsClient : IProductsClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsClient> _logger;

    public ProductsClient(HttpClient httpClient, IConfiguration configuration, ILogger<ProductsClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        // Dirección y timeout de Products.API, desde appsettings.json
        string baseUrl = configuration["Services:ProductsApi:BaseUrl"];
        if (string.IsNullOrEmpty(baseUrl))
        {
            baseUrl = "http://localhost:5001/";
        }
        int timeoutSegundos = configuration.GetValue<int>("Services:ProductsApi:TimeoutSeconds", 5);

        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSegundos);
    }

    public async Task<ProductDto> GetByIdAsync(Guid productoId)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync("api/products/" + productoId);
        }
        catch (HttpRequestException ex)
        {
            // Products caído o puerto equivocado
            _logger.LogError(ex, "No se pudo comunicar con Products.API al buscar el producto {ProductoId}", productoId);
            throw new ExternalServiceException(ErrorCodes.CRT_005,
                "No se pudo comunicar con Products.API para validar el producto.");
        }
        catch (TaskCanceledException ex)
        {
            // Products no respondió a tiempo (timeout)
            _logger.LogError(ex, "Products.API no respondió a tiempo al buscar el producto {ProductoId}", productoId);
            throw new ExternalServiceException(ErrorCodes.CRT_005,
                "No se pudo comunicar con Products.API para validar el producto.");
        }

        using (response)
        {
            // 404: el producto no existe → el service responde CRT-002
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Products.API respondió 404 para el producto {ProductoId}", productoId);
                return null;
            }

            // Cualquier otro error (500, 503, etc.) → CRT-005
            if (!response.IsSuccessStatusCode)
            {
                int status = (int)response.StatusCode;
                _logger.LogError("Products.API respondió {StatusCode} al buscar el producto {ProductoId}", status, productoId);
                throw new ExternalServiceException(ErrorCodes.CRT_005,
                    "Products.API respondió con un error (" + status + ") al validar el producto.");
            }

            ProductDto product = await response.Content.ReadFromJsonAsync<ProductDto>();
            if (product == null)
            {
                throw new ExternalServiceException(ErrorCodes.CRT_005, "Products.API devolvió una respuesta vacía.");
            }

            return product;
        }
    }
}
