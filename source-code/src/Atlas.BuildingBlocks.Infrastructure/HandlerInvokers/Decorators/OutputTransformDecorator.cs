using Atlas.BuildingBlocks.Application.HandlerInvokers.Interfaces;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers.Decorators;

/// <summary>
/// Type boundary between the raw-handler world and the result world.
///
/// Responsibility: happy path only — wraps TOutput in Result.Ok.
/// Exceptions are NOT caught here; they propagate to
/// <see cref="DomainExceptionDecorator{TInput,TOutput}"/> which sits above this in the pipeline.
/// </summary>
internal sealed class OutputTransformDecorator<TInput, TOutput> : IResultPipelineStep<TInput, TOutput>
{
    private readonly IHandler<TInput, TOutput> _inner;

    public OutputTransformDecorator(IHandler<TInput, TOutput> inner) => _inner = inner;

    public async Task<Result<TOutput>> ExecuteAsync(TInput input, CancellationToken ct)
        => Result.Ok(await _inner.ExecuteAsync(input, ct));
}
