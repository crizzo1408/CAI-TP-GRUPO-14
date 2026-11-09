namespace Notifications.API.Exceptions;

public class ValidationException : DomainException
{
    public ValidationException(string errorCode, string message) : base(errorCode, message)
    {
    }
}
