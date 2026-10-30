using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Cart.API.HealthChecks;

/// <summary>
/// Verifica que Products.API responda en /health/live.
/// Si no responde, Cart queda Degraded: sigue funcionando para consultar y borrar,
/// pero no puede agregar ni modificar productos.
/// </summary>
public class ProductsApiHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var baseUrl = configuration["Services:ProductsApi:BaseUrl"];
        try
        {
            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            using var response = await client.GetAsync(new Uri(new Uri(baseUrl!), "health/live"), cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy($"Products.API responde en {baseUrl}")
                : new HealthCheckResult(context.Registration.FailureStatus,
                    $"Products.API respondió {(int)response.StatusCode}");
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus,
                $"Products.API no responde en {baseUrl}", ex);
        }
    }
}
