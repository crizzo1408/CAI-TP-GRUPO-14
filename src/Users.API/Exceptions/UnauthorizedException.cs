namespace Users.API.Exceptions;

/// <summary>Credenciales incorrectas → HTTP 401.</summary>
public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string errorCode, string message) : base(errorCode, message)
    {
    }
}
