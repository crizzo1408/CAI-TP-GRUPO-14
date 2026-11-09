using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Notifications.API.Models;

namespace Notifications.API.Data;

public class NotificationRow
{
    public string Id { get; set; }
    public string UsuarioId { get; set; }
    public string Mensaje { get; set; }
    public string Tipo { get; set; }
    public string Estado { get; set; }
    public string FechaEnvio { get; set; }
}

public class NotificationRepository : INotificationRepository
{
    private readonly IConfiguration _configuration;

    public NotificationRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_configuration.GetConnectionString("DefaultConnection"));
    }

    public async Task CreateAsync(Notification notification)
    {
        using var connection = CreateConnection();
        await connection.ExecuteAsync(@"
            INSERT INTO notifications (id, usuario_id, mensaje, tipo, estado, fecha_envio)
            VALUES (@Id, @UsuarioId, @Mensaje, @Tipo, @Estado, @FechaEnvio);",
            new
            {
                Id = notification.Id.ToString(),
                UsuarioId = notification.UsuarioId.ToString(),
                notification.Mensaje,
                notification.Tipo,
                notification.Estado,
                FechaEnvio = notification.FechaEnvio.ToString("o")
            });
    }

    public async Task<List<Notification>> GetByUserIdAsync(Guid usuarioId)
    {
        using var connection = CreateConnection();
        IEnumerable<NotificationRow> rows = await connection.QueryAsync<NotificationRow>(@"
            SELECT id          AS Id,
                   usuario_id  AS UsuarioId,
                   mensaje     AS Mensaje,
                   tipo        AS Tipo,
                   estado      AS Estado,
                   fecha_envio AS FechaEnvio
            FROM notifications
            WHERE usuario_id = @UsuarioId
            ORDER BY fecha_envio DESC",
            new { UsuarioId = usuarioId.ToString() });

        var notifications = new List<Notification>();
        foreach (NotificationRow row in rows)
        {
            notifications.Add(ConvertToNotification(row));
        }

        return notifications;
    }

    private static Notification ConvertToNotification(NotificationRow row)
    {
        var notification = new Notification();
        notification.Id = Guid.Parse(row.Id);
        notification.UsuarioId = Guid.Parse(row.UsuarioId);
        notification.Mensaje = row.Mensaje;
        notification.Tipo = row.Tipo;
        notification.Estado = row.Estado;
        notification.FechaEnvio = DateTime.Parse(row.FechaEnvio, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        return notification;
    }
}
