using Cart.API.Middleware;

namespace Cart.API.Clients;

/// <summary>
/// Se ejecuta en cada llamada HTTP saliente (a Products.API) y agrega el header X-Correlation-Id
/// con el mismo id del request actual. Así el request se puede seguir en los logs de ambos servicios.
/// </summary>
public class CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.ItemKey] is string correlationId
            && !request.Headers.Contains(CorrelationIdMiddleware.HeaderName))
        {
            request.Headers.Add(CorrelationIdMiddleware.HeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
