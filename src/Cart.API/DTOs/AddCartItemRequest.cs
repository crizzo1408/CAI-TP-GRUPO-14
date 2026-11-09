using System.ComponentModel.DataAnnotations;

namespace Cart.API.DTOs;

/// <summary>
/// Producto a agregar al carrito.
/// </summary>
public class AddCartItemRequest
{
    /// <summary>Id del producto (debe existir en Products.API).</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    [Required(ErrorMessage = "El productoId es obligatorio.")]
    public Guid? ProductoId { get; set; }

    /// <summary>Cantidad a agregar. Debe ser mayor a cero. Si el producto ya estaba en el carrito, se suma.</summary>
    /// <example>2</example>
    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    public int? Cantidad { get; set; }
}
