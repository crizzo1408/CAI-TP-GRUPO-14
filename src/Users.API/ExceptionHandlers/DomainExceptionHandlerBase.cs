using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

/// <summary>
/// Lógica común de los handlers de excepciones de negocio.
/// Cada handler concreto solo indica qué excepción maneja y con qué status HTTP responde.
/// </summary>
public abstract class DomainExceptionHandlerBase<TException>(ILogger logger) : IExceptionHandler
    where TException : DomainException
{
    /// <summary>Status HTTP con el que se responde.</summary>
    protected abstract int StatusCode { get; }

    /// <summary>Nivel del log. Errores de negocio → Warning (lo pide la consigna).</summary>
    protected virtual LogLevel LogLevel => LogLevel.Warning;

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        // Si no es la excepción que este handler sabe manejar, devuelve false
        // y ASP.NET prueba con el siguiente handler registrado.
        if (exception is not TException ex) return false;

        logger.Log(LogLevel, "Error {ErrorCode} ({StatusCode}): {ErrorMessage}", ex.ErrorCode, StatusCode, ex.Message);

        var error = ErrorResponseFactory.Create(context, StatusCode, ex.ErrorCode, ex.Message);
        await ErrorResponseFactory.WriteAsync(context, error, cancellationToken);
        return true;
    }
}
