using Atlas.SharedKernel.Application.Errors;

namespace Atlas.SharedKernel.Application;

public readonly struct Unit
{
    public static readonly Unit Value = default;
}

public sealed class Response : IResponse
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public ErrorDefinition? ErrorDefinition { get; }
    public object? Value { get; }

    private Response(bool isSuccess, object? value, ErrorDefinition? errorDefinition, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorDefinition = errorDefinition;
        Error = error;
    }

    public static Response Ok<T>(T value)
        => new(true, value, null, null);

    public static Response Failure(ErrorDefinition definition, string? message = null)
        => new(false, Unit.Value, definition, message ?? definition.FallbackMessage);

    public object? GetValue() => Value;
}
