using Cart.API.Models;

namespace Cart.API.Data;

/// <summary>
/// Acceso a datos del carrito. El service solo conoce esta interfaz, no SQL.
/// </summary>
public interface ICartRepository
{
    /// <summary>Devuelve el carrito con sus items, o null si el usuario no tiene carrito.</summary>
    Task<ShoppingCart?> GetAsync(Guid usuarioId);

    /// <summary>Crea el carrito si no existe y guarda el item con la cantidad indicada (insert o update).</summary>
    Task SaveItemAsync(Guid usuarioId, Guid productoId, int cantidad, DateTime fechaActualizacion);

    /// <summary>Quita un producto del carrito.</summary>
    Task RemoveItemAsync(Guid usuarioId, Guid productoId, DateTime fechaActualizacion);

    /// <summary>Quita todos los productos del carrito (el carrito sigue existiendo, vacío).</summary>
    Task ClearAsync(Guid usuarioId, DateTime fechaActualizacion);
}
