namespace Cart.API.Constants;

/// <summary>
/// Catálogo de códigos de error de Cart.API (sección 4.4 de la consigna).
/// Se usan constantes para no tener strings sueltos en el código.
/// </summary>
public static class ErrorCodes
{
    /// <summary>404 - Carrito no encontrado.</summary>
    public const string CRT_001 = "CRT-001";
    /// <summary>404 - Producto no encontrado (no existe en Products.API).</summary>
    public const string CRT_002 = "CRT-002";
    /// <summary>422 - Stock insuficiente para agregar al carrito.</summary>
    public const string CRT_003 = "CRT-003";
    /// <summary>400 - Cantidad inválida (y cualquier otro dato inválido del request).</summary>
    public const string CRT_004 = "CRT-004";
    /// <summary>500 - Error interno al procesar el carrito (incluye Products.API caído).</summary>
    public const string CRT_005 = "CRT-005";
}

/// <summary>
/// Mensajes de error (errorMessage) asociados a cada código.
/// </summary>
public static class ErrorMessages
{
    public const string CRT_001 = "Carrito no encontrado.";
    public const string CRT_002 = "Producto no encontrado.";
    public const string CRT_003 = "Stock insuficiente para agregar al carrito.";
    public const string CRT_004 = "Cantidad inválida.";
    public const string CRT_005 = "Error interno al procesar el carrito.";
}
