using System.Net.Http.Json;

namespace MiniApi.Clients;

public class OrdersClient
{
    private readonly HttpClient _httpClient;

    private readonly IHttpContextAccessor
        _httpContextAccessor;

    public OrdersClient(
        HttpClient httpClient,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;

        var baseUrl =
            configuration["Services:OrdersApi"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "http://localhost:5003";
        }

        if (baseUrl.EndsWith("/") == false)
        {
            baseUrl = baseUrl + "/";
        }

        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<bool> HasActiveOrdersAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "api/orders");

        AddCorrelationId(request);

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var orders =
            await response.Content
                .ReadFromJsonAsync<List<OrderResponse>>(
                    cancellationToken:
                        cancellationToken);

        if (orders == null)
        {
            return false;
        }

        foreach (var order in orders)
        {
            var isActive = IsActiveOrder(order.Estado);

            if (isActive)
            {
                foreach (var item in order.Items)
                {
                    if (item.ProductoId == productId)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void AddCorrelationId(
        HttpRequestMessage request)
    {
        var currentContext =
            _httpContextAccessor.HttpContext;

        if (currentContext == null)
        {
            return;
        }

        var correlationId =
            currentContext.TraceIdentifier;

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            return;
        }

        request.Headers.Add(
            "X-Correlation-Id",
            correlationId);
    }

    private static bool IsActiveOrder(string estado)
    {
        if (estado.Equals(
            "Pendiente",
            StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        else if (estado.Equals(
            "Confirmada",
            StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private class OrderResponse
    {
        public List<OrderItemResponse> Items { get; set; }
            = new List<OrderItemResponse>();

        public string Estado { get; set; }
            = string.Empty;
    }

    private class OrderItemResponse
    {
        public Guid ProductoId { get; set; }
    }
}