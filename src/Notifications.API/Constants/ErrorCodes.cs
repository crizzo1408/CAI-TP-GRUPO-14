namespace Notifications.API.Constants;

/// <summary>Catálogo de errores de Notifications.API.</summary>
public static class ErrorCodes
{
    /// <summary>404 - El usuario destinatario no existe.</summary>
    public const string NTF_001 = "NTF-001";
    /// <summary>400 - Los datos de la notificación son inválidos.</summary>
    public const string NTF_002 = "NTF-002";
    /// <summary>404 - El usuario no tiene notificaciones.</summary>
    public const string NTF_003 = "NTF-003";
    /// <summary>500 - Error interno del servicio.</summary>
    public const string NTF_004 = "NTF-004";
}

public static class ErrorMessages
{
    public const string NTF_001 = "Usuario no encontrado.";
    public const string NTF_002 = "Datos inválidos.";
    public const string NTF_003 = "No hay notificaciones para el usuario.";
    public const string NTF_004 = "Error interno al procesar la notificación.";
}
