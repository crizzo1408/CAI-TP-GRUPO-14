namespace Users.API.Exceptions;

/// <summary>Recurso inexistente (usuario no encontrado) → HTTP 404.</summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string errorCode, string message) : base(errorCode, message)
    {
    }
}
