namespace Users.API.Constants;

/// <summary>
/// Catálogo de códigos de error de Users.API (sección 4.2 de la consigna).
/// Se usan constantes para no tener strings sueltos en el código.
/// </summary>
public static class ErrorCodes
{
    /// <summary>409 - El email ya está registrado.</summary>
    public const string USR_001 = "USR-001";
    /// <summary>400 - Los datos del usuario son inválidos.</summary>
    public const string USR_002 = "USR-002";
    /// <summary>401 - Credenciales incorrectas.</summary>
    public const string USR_003 = "USR-003";
    /// <summary>403 - Usuario bloqueado por demasiados intentos fallidos.</summary>
    public const string USR_004 = "USR-004";
    /// <summary>403 - Usuario bloqueado por detección de fraude (bloqueo manual).</summary>
    public const string USR_005 = "USR-005";
    /// <summary>500 - Error interno al procesar el usuario.</summary>
    public const string USR_006 = "USR-006";
    /// <summary>
    /// 404 - Usuario no encontrado (GET /api/users/{id}).
    /// EXTENSIÓN: no está en la consigna; se agregó para que Orders y Notifications
    /// puedan validar que un usuario exista. A confirmar con el docente.
    /// </summary>
    public const string USR_007 = "USR-007";
}

/// <summary>
/// Mensajes de error (errorMessage) asociados a cada código.
/// </summary>
public static class ErrorMessages
{
    public const string USR_001 = "El email ya está registrado.";
    public const string USR_002 = "Los datos del usuario son inválidos.";
    public const string USR_003 = "Credenciales incorrectas.";
    public const string USR_004 = "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.";
    public const string USR_005 = "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.";
    public const string USR_006 = "Error interno al procesar el usuario.";
    public const string USR_007 = "Usuario no encontrado.";
}
