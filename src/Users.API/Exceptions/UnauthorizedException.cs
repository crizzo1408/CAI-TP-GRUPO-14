namespace Users.API.Exceptions;

/// <summary>Credenciales incorrectas → HTTP 401.</summary>
public class UnauthorizedException(string errorCode, string message) : DomainException(errorCode, message);
