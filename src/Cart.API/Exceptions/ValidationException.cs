namespace Cart.API.Exceptions;

/// <summary>Datos de entrada inválidos → HTTP 400.</summary>
public class ValidationException(string errorCode, string message) : DomainException(errorCode, message);
