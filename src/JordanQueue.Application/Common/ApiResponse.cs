namespace JordanQueue.Application.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public IReadOnlyList<ApiError> Errors { get; set; } = Array.Empty<ApiError>();

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, params ApiError[] errors) =>
        new() { Success = false, Message = message, Errors = errors };
}

public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
