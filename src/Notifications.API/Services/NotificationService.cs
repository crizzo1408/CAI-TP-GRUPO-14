using Notifications.API.Clients;
using Notifications.API.Constants;
using Notifications.API.Data;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;

namespace Notifications.API.Services;

public class NotificationService : INotificationService
{
    private static readonly string[] ValidTypes = { "Email", "Push", "SMS" };

    private readonly INotificationRepository _repository;
    private readonly IUsersClient _usersClient;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repository,
        IUsersClient usersClient,
        ILogger<NotificationService> logger)
    {
        _repository = repository;
        _usersClient = usersClient;
        _logger = logger;
    }

    public async Task<NotificationResponse> SendAsync(
        SendNotificationRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        bool userExists = await _usersClient.ExistsAsync(request.UsuarioId, cancellationToken);
        if (!userExists)
        {
            throw new NotFoundException(ErrorCodes.NTF_001, ErrorMessages.NTF_001);
        }

        var notification = new Notification();
        notification.Id = Guid.NewGuid();
        notification.UsuarioId = request.UsuarioId;
        notification.Mensaje = request.Mensaje.Trim();
        notification.Tipo = NormalizeType(request.Tipo);
        notification.Estado = "Enviada";
        notification.FechaEnvio = DateTime.UtcNow;

        await _repository.CreateAsync(notification);

        _logger.LogInformation(
            "Notificación {NotificationId} enviada al usuario {UserId} por {Type}",
            notification.Id,
            notification.UsuarioId,
            notification.Tipo);

        return ToResponse(notification);
    }

    public async Task<List<NotificationResponse>> GetByUserIdAsync(Guid usuarioId)
    {
        if (usuarioId == Guid.Empty)
        {
            throw new ValidationException(ErrorCodes.NTF_002, "El usuarioId debe ser un Guid válido.");
        }

        List<Notification> notifications = await _repository.GetByUserIdAsync(usuarioId);
        if (notifications.Count == 0)
        {
            throw new NotFoundException(ErrorCodes.NTF_003, ErrorMessages.NTF_003);
        }

        var response = new List<NotificationResponse>();
        foreach (Notification notification in notifications)
        {
            response.Add(ToResponse(notification));
        }

        return response;
    }

    private static void ValidateRequest(SendNotificationRequest request)
    {
        if (request.UsuarioId == Guid.Empty)
        {
            throw new ValidationException(ErrorCodes.NTF_002, "El usuarioId debe ser un Guid válido.");
        }

        if (string.IsNullOrWhiteSpace(request.Mensaje))
        {
            throw new ValidationException(ErrorCodes.NTF_002, "El mensaje es obligatorio.");
        }

        if (request.Mensaje.Length > 500)
        {
            throw new ValidationException(ErrorCodes.NTF_002, "El mensaje no puede superar los 500 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(request.Tipo) || NormalizeType(request.Tipo) == null)
        {
            throw new ValidationException(ErrorCodes.NTF_002, "El tipo debe ser Email, Push o SMS.");
        }
    }

    private static string NormalizeType(string type)
    {
        foreach (string validType in ValidTypes)
        {
            if (validType.Equals(type?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return validType;
            }
        }

        return null;
    }

    private static NotificationResponse ToResponse(Notification notification)
    {
        var response = new NotificationResponse();
        response.Id = notification.Id;
        response.UsuarioId = notification.UsuarioId;
        response.Mensaje = notification.Mensaje;
        response.Tipo = notification.Tipo;
        response.Estado = notification.Estado;
        response.FechaEnvio = notification.FechaEnvio;
        return response;
    }
}
