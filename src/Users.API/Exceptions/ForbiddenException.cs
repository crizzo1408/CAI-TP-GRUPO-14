namespace Users.API.Exceptions;

/// <summary>Acceso prohibido (usuario bloqueado) → HTTP 403.</summary>
public class ForbiddenException : DomainException
{
    public ForbiddenException(string errorCode, string message) : base(errorCode, message)
    {
    }
}
