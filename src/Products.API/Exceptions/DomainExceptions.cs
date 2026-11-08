namespace MiniApi.Exceptions;

public sealed class NotFoundException : Exception
{
    public string ErrorCode { get; }

    public NotFoundException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}

public sealed class ValidationException : Exception
{
    public string ErrorCode { get; }

    public ValidationException(string errorCode, List<string> errors)
        : base(BuildMessage(errors))
    {
        ErrorCode = errorCode;
    }

    private static string BuildMessage(List<string> errors)
    {
        if (errors.Count == 0)
        {
            return "Los datos del producto son inválidos.";
        }

        var message = string.Empty;

        foreach (var error in errors)
        {
            if (message.Length > 0)
            {
                message += "; ";
            }

            message += error;
        }

        return message;
    }
}

public sealed class BusinessRuleException : Exception
{
    public string ErrorCode { get; }

    public BusinessRuleException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}