namespace JordanQueue.Application.Exceptions;

public class AppException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }

    public AppException(string message, string code = "APP_ERROR", int statusCode = 400)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}

public class NotFoundException : AppException
{
    public NotFoundException(string message, string code = "NOT_FOUND")
        : base(message, code, 404)
    {
    }
}

public class ValidationException : AppException
{
    public IReadOnlyList<string> ValidationErrors { get; }

    public ValidationException(IEnumerable<string> errors)
        : base("Validation failed.", "VALIDATION_ERROR", 400)
    {
        ValidationErrors = errors.ToList();
    }
}

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Unauthorized.", string code = "UNAUTHORIZED")
        : base(message, code, 401)
    {
    }
}

public class ConflictException : AppException
{
    public ConflictException(string message, string code = "CONFLICT")
        : base(message, code, 409)
    {
    }
}
