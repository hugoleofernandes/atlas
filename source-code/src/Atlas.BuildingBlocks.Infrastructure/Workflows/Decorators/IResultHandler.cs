using Atlas.SharedKernel.Application.Commands;

namespace Atlas.BuildingBlocks.Infrastructure.Workflows.Decorators;

/// <summary>
/// Internal boundary interface between the raw-handler world (IHandler returning TOutput)
/// and the result world (returning Result&lt;TOutput&gt;).
///
/// Implemented by: ResultTransformDecorator, DomainExceptionDecorator,
/// LoggingDecorator, TelemetryDecorator.
/// </summary>
internal interface IResultHandler<TInput, TOutput>
{
    Task<Result<TOutput>> ExecuteAsync(TInput input, CancellationToken ct);
}
