using Atlas.BuildingBlocks.Application.HandlerInvokers.Interfaces;
using Atlas.BuildingBlocks.Infrastructure.Validation;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;
using FluentValidation;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers.Decorators;

/// <summary>
/// Catches known failure exceptions thrown below in the pipeline and converts them to Result.Fail.
///
/// Three cases handled:
///   <see cref="ValidationException"/>    → validation errors from FluentValidation
///   <see cref="DomainException"/>        → explicit domain rule violations
///   <see cref="HandlerResultException"/> → re-surfaced Result.Fail from a nested command handler
///                                          invocation inside an integration event adapter;
///                                          preserves the original <see cref="ErrorDefinition"/>
///                                          so <see cref="LoggingDecorator{TInput,TOutput}"/> and
///                                          <see cref="TelemetryDecorator{TInput,TOutput}"/> receive
///                                          the structured failure (code + category) upstream.
///
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
        catch (HandlerResultException ex)
        {
            return Result.Fail<TOutput>(ex.ErrorDefinition);
        }
    }
}
