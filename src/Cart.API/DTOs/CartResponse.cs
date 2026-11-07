namespace Cart.API.DTOs;

/// <summary>
/// Carrito del usuario.
/// </summary>
public class CartResponse
{
    /// <example>a1b2c3d4-0000-0000-0000-111122223333</example>
    public Guid UsuarioId { get; set; }

    public List<CartItemResponse> Items { get; set; } = new List<CartItemResponse>();

    /// <example>2024-03-10T10:45:00Z</example>
    public DateTime FechaActualizacion { get; set; }
}

/// <summary>
/// Producto dentro del carrito.
/// </summary>
public class CartItemResponse
{
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid ProductoId { get; set; }

    /// <example>2</example>
    public int Cantidad { get; set; }
}
