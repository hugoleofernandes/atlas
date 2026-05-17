using Atlas.SharedKernel.Application.Errors;

namespace Atlas.SharedKernel.Application.Commands;

public interface IResultService
{
    Result<T> Success<T>(T value);

    Result<T> Failure<T>(ErrorDefinition error);
}