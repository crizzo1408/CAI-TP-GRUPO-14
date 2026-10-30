namespace Users.API.Exceptions;

/// <summary>Conflicto con datos existentes (email duplicado) → HTTP 409.</summary>
public class ConflictException(string errorCode, string message) : DomainException(errorCode, message);
