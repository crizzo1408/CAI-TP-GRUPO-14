using System.ComponentModel.DataAnnotations;

namespace Cart.API.DTOs;

/// <summary>
/// Nueva cantidad de un producto del carrito.
/// </summary>
public class UpdateCartItemRequest
{
    /// <summary>Nueva cantidad (reemplaza la anterior). Debe ser mayor a cero.</summary>
    /// <example>4</example>
    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    public int? Cantidad { get; set; }
}
