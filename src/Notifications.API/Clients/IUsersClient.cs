namespace Notifications.API.Clients;

public interface IUsersClient
{
    Task<bool> ExistsAsync(Guid usuarioId, CancellationToken cancellationToken);
}
