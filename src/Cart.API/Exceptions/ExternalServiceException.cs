namespace Cart.API.Exceptions;

/// <summary>Falla al comunicarse con otro microservicio (Products.API) → HTTP 500.</summary>
public class ExternalServiceException(string errorCode, string message) : DomainException(errorCode, message);
