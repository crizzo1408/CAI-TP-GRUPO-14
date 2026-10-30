namespace Cart.API.Exceptions;

/// <summary>Recurso inexistente (carrito o producto) → HTTP 404.</summary>
public class NotFoundException(string errorCode, string message) : DomainException(errorCode, message);
