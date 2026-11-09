using Notifications.API.DTOs;

namespace Notifications.API.Services;

public interface INotificationService
{
    Task<NotificationResponse> SendAsync(SendNotificationRequest request, CancellationToken cancellationToken);
    Task<List<NotificationResponse>> GetByUserIdAsync(Guid usuarioId);
}
