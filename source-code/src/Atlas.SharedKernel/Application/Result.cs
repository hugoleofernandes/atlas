namespace Atlas.SharedKernel.Application;

public sealed class Result<T> : IResult
{
    public bool Success { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }
    public T? Value { get; }

    private Result(bool success, T? value, string? error, string? errorCode)
    {
        Success = success;
        Value = value;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result<T> Ok(T value)
        => new(true, value, null, null);

    public static Result<T> Failure(string error, string errorCode)
        => new(false, default, error, errorCode);
}