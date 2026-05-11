using Atlas.SharedKernel.Application.Errors;

namespace Atlas.SharedKernel.Application.UseCases;

public sealed class Result<T>
{
    public bool IsSuccess { get; }

    public T? Value { get; }

    public ErrorDefinition? Error { get; }

    internal Result(
        bool isSuccess,
        T? value,
        ErrorDefinition? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
}