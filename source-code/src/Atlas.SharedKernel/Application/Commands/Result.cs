using Atlas.SharedKernel.Application.Errors;

namespace Atlas.SharedKernel.Application.Commands;

/// <summary>
/// Generic result container. Use the non-generic Result factory to create instances:
///   Result.Ok(value)         — T is inferred from the argument
///   Result.Fail&lt;T&gt;(error)    — T must be specified explicitly
/// </summary>
public sealed class Result<T> : IResponse
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ErrorDefinition? ErrorDefinition { get; }

    // IResponse — human-readable fallback used when no localizer is available
    public string? Error => ErrorDefinition?.FallbackMessage;

    internal Result(bool isSuccess, T? value, ErrorDefinition? errorDefinition)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorDefinition = errorDefinition;
    }

    public object? GetValue() => Value;
}

/// <summary>
/// Static factory for Result&lt;T&gt;.
/// Keeping factory methods here (non-generic class) allows the compiler
/// to infer T on Ok calls: Result.Ok(output) instead of Result&lt;Output&gt;.Ok(output).
/// </summary>
public static class Result
{
    public static Result<T> Ok<T>(T value)
        => new(true, value, null);

    public static Result<T> Fail<T>(ErrorDefinition error)
        => new(false, default, error);
}
