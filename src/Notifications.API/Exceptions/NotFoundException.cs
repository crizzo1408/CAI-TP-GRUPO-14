namespace Notifications.API.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string errorCode, string message) : base(errorCode, message)
    {
    }
}
