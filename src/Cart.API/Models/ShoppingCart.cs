namespace Cart.API.Models;

/// <summary>
/// Entidad de dominio Cart (Apéndice A de la consigna). Un carrito por usuario.
/// Se llama ShoppingCart para no chocar con el namespace Cart.API.
/// </summary>
public class ShoppingCart
{
    public Guid UsuarioId { get; set; }
    public List<CartItem> Items { get; set; } = [];

    /// <summary>Se actualiza automáticamente en cada operación.</summary>
    public DateTime FechaActualizacion { get; set; }
}

/// <summary>
/// Producto dentro del carrito.
/// </summary>
public class CartItem
{
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
}
