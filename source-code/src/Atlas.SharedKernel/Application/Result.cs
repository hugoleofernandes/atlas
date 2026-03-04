using Atlas.SharedKernel.Application.Errors;

namespace Atlas.SharedKernel.Application;

public sealed class Result<T> : IResult
{
    public bool Success { get; }

    public string? Error { get; }

    public ErrorDefinition? ErrorDefinition { get; }

    public T? Value { get; }

    private Result(
        bool success,
        T? value,
        ErrorDefinition? errorDefinition,
        string? error)
    {
        Success = success;
        Value = value;
        ErrorDefinition = errorDefinition;
        Error = error;
    }

    public static Result<T> Ok(T value)
        => new(true, value, null, null);

    public static Result<T> Failure(
        ErrorDefinition definition,
        string? message = null)
        => new(
            false,
            default,
            definition,
            message ?? definition.DefaultMessage
        );

    public object? GetValue() => Value;
}