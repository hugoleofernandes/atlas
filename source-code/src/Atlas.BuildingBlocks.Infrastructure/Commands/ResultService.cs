using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Errors;

namespace Atlas.BuildingBlocks.Application.Commands;

public sealed class ResultService : IResultService
{
    public Result<T> Success<T>(T value)
        => new(true, value, null);

    public Result<T> Failure<T>(ErrorDefinition error)
        => new(false, default, error);
}