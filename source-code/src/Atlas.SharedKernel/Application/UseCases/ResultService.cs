using Atlas.SharedKernel.Application.Errors;

namespace Atlas.SharedKernel.Application.UseCases;

public sealed class ResultService : IResultService
{
    public Result<T> Success<T>(T value)
        => new(true, value, null);

    public Result<T> Failure<T>(ErrorDefinition error)
        => new(false, default, error);
}