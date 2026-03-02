namespace Atlas.SharedKernel.Application;

public sealed class Result<T>
{
    public bool Success { get; }
    public T? Data { get; }
    public List<string> Errors { get; }

    private Result(bool success, T? data, List<string> errors)
    {
        Success = success;
        Data = data;
        Errors = errors;
    }

    public static Result<T> Ok(T data)
        => new(true, data, new());

    public static Result<T> Failure(params string[] errors)
        => new(false, default, errors.ToList());
}