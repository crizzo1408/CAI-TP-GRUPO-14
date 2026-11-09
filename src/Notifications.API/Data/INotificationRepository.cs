using Notifications.API.Models;

namespace Notifications.API.Data;

public interface INotificationRepository
{
    Task CreateAsync(Notification notification);
    Task<List<Notification>> GetByUserIdAsync(Guid usuarioId);
}
