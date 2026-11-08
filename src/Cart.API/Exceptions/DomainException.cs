namespace Cart.API.Exceptions;

/// <summary>
/// Clase base de todas las excepciones de negocio.
/// Lleva el código de error del catálogo (ej: CRT-003) y el mensaje.
/// Los services las lanzan y los IExceptionHandler las convierten en la respuesta HTTP.
/// </summary>
public abstract class DomainException : Exception
{
    public string ErrorCode { get; }

    protected DomainException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}
