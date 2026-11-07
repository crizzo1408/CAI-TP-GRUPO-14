namespace Cart.API.Exceptions;

/// <summary>Datos de entrada inválidos → HTTP 400.</summary>
public class ValidationException : DomainException
{
    public ValidationException(string errorCode, string message) : base(errorCode, message)
    {
    }
}
