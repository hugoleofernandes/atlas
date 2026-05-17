using Atlas.SharedKernel.Application.Errors;

namespace Atlas.SharedKernel.Application.Commands;

public sealed class Result<T>
{
    public bool IsSuccess { get; }

    public T? Value { get; }

    public ErrorDefinition? Error { get; }

    public Result(
        bool isSuccess,
        T? value,
        ErrorDefinition? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
}