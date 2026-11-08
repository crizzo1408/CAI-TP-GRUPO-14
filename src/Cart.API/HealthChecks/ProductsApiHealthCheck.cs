using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Cart.API.HealthChecks;

/// <summary>
/// Verifica que Products.API responda en /health/live.
/// Si no responde, Cart queda Degraded: sigue funcionando para consultar y borrar,
/// pero no puede agregar ni modificar productos.
/// </summary>
public class ProductsApiHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ProductsApiHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        string baseUrl = _configuration["Services:ProductsApi:BaseUrl"];
        try
        {
            HttpClient client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(3);

            var url = new Uri(new Uri(baseUrl), "health/live");
            using HttpResponseMessage response = await client.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("Products.API responde en " + baseUrl);
            }

            // FailureStatus es Degraded (se configura al registrar el check)
            return new HealthCheckResult(context.Registration.FailureStatus,
                "Products.API respondió " + (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus,
                "Products.API no responde en " + baseUrl, ex);
        }
    }
}
