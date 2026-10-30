namespace Users.API.Exceptions;

/// <summary>Acceso prohibido (usuario bloqueado) → HTTP 403.</summary>
public class ForbiddenException(string errorCode, string message) : DomainException(errorCode, message);
