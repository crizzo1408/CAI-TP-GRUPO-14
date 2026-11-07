using Cart.API.Middleware;

namespace Cart.API.Clients;

/// <summary>
/// Se ejecuta en cada llamada HTTP saliente (a Products.API) y agrega el header X-Correlation-Id
/// con el mismo id del request actual. Así el request se puede seguir en los logs de ambos servicios.
/// </summary>
public class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpContext context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            var correlationId = context.Items[CorrelationIdMiddleware.ItemKey] as string;
            if (correlationId != null && !request.Headers.Contains(CorrelationIdMiddleware.HeaderName))
            {
                request.Headers.Add(CorrelationIdMiddleware.HeaderName, correlationId);
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}
