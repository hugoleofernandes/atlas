using Atlas.SharedKernel.Application.Errors;

namespace Atlas.SharedKernel.Application.UseCases;

public interface IResultService
{
    Result<T> Success<T>(T value);

    Result<T> Failure<T>(ErrorDefinition error);
}