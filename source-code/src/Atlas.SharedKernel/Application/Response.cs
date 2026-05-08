using Atlas.SharedKernel.Application.Errors;

namespace Atlas.SharedKernel.Application;

public sealed class Response<T> : IResponse
{
    public bool IsSuccess { get; }

    public string? Error { get; }

    public ErrorDefinition? ErrorDefinition { get; }

    public T? Value { get; }

    private Response(
        bool isSuccess,
        T? value,
        ErrorDefinition? errorDefinition,
        string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorDefinition = errorDefinition;
        Error = error;
    }

    public static Response<T> Ok(T value)
        => new(true, value, null, null);

    public static Response<T> Failure(
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