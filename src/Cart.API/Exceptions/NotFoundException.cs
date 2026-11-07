namespace Cart.API.Exceptions;

/// <summary>Recurso inexistente (carrito o producto) → HTTP 404.</summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string errorCode, string message) : base(errorCode, message)
    {
    }
}
