using Atlas.BuildingBlocks.Infrastructure.Validation;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;
using FluentValidation;

namespace Atlas.BuildingBlocks.Application.Invokers.Decorators;

/// <summary>
/// Catches <see cref="DomainException"/> and <see cref="ValidationException"/> thrown
/// anywhere below in the pipeline and converts them to Result.Fail.
///
/// Responsibility: sad path only — DomainException and ValidationException → Result.Fail.
/// The happy path (Result.Ok) is owned by <see cref="ResultTransformDecorator{TInput,TOutput}"/>.
/// Unexpected exceptions are NOT caught — they propagate to
/// <see cref="LoggingDecorator{TInput,TOutput}"/> for error logging and re-throw.
/// </summary>
internal sealed class DomainExceptionDecorator<TInput, TOutput> : IResultPipelineStep<TInput, TOutput>
{
    private readonly IResultPipelineStep<TInput, TOutput> _inner;

    public DomainExceptionDecorator(IResultPipelineStep<TInput, TOutput> inner) => _inner = inner;

    public async Task<Result<TOutput>> ExecuteAsync(TInput input, CancellationToken ct)
    {
        try
        {
            return await _inner.ExecuteAsync(input, ct);
        }
        catch (ValidationException ex)
        {
            return Result.Fail<TOutput>(ex.ToErrorDefinition());
        }
        catch (DomainException ex)
        {
            return Result.Fail<TOutput>(new ErrorDefinition(ex.ErrorCode, ex.Message, ex.Category));
        }
    }
}
