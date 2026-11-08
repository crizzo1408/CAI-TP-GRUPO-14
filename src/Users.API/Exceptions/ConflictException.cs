namespace Users.API.Exceptions;

/// <summary>Conflicto con datos existentes (email duplicado) → HTTP 409.</summary>
public class ConflictException : DomainException
{
    public ConflictException(string errorCode, string message) : base(errorCode, message)
    {
    }
}
