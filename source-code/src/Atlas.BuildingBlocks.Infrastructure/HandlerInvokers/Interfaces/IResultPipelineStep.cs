using Atlas.SharedKernel.Application.Commands;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers.Interfaces;

/// <summary>
/// Internal boundary interface between the raw-handler world (IHandler returning TOutput)
/// and the result world (returning Result&lt;TOutput&gt;).
///
/// Implemented by: OutputTransformDecorator, DomainExceptionDecorator,
/// LoggingDecorator, TelemetryDecorator.
/// </summary>
internal interface IResultPipelineStep<TInput, TOutput>
{
    Task<Result<TOutput>> ExecuteAsync(TInput input, CancellationToken ct);
}
