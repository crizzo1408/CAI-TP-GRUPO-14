namespace Cart.API.Exceptions;

/// <summary>Violación de una regla de negocio (stock insuficiente) → HTTP 422.</summary>
public class BusinessRuleException(string errorCode, string message) : DomainException(errorCode, message);
