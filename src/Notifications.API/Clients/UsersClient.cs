using System.Net.Http.Json;
using Notifications.API.DTOs;
using Notifications.API.Middleware;

namespace Notifications.API.Clients;

/// <summary>Cliente HTTP para validar usuarios sin acceder a la base de Users.API.</summary>
public class UsersClient : IUsersClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsersClient(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;

        string baseUrl = configuration["Services:UsersApi"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "http://localhost:5002";
        }

        if (!baseUrl.EndsWith("/"))
        {
            baseUrl = baseUrl + "/";
        }

        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<bool> ExistsAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/users/" + usuarioId + "/exists");
        AddCorrelationId(request);

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        UserExistsResponse result = await response.Content.ReadFromJsonAsync<UserExistsResponse>(
            cancellationToken: cancellationToken);

        if (result == null)
        {
            throw new InvalidOperationException("Users.API devolvió una respuesta vacía.");
        }

        return result.Exists;
    }

    private void AddCorrelationId(HttpRequestMessage request)
    {
        HttpContext context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return;
        }

        string correlationId = context.Items[CorrelationIdMiddleware.ItemKey] as string;
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = context.TraceIdentifier;
        }

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            request.Headers.Add(CorrelationIdMiddleware.HeaderName, correlationId);
        }
    }
}
