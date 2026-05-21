using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers.Interfaces;

/// <summary>
/// Executes any handler through an explicit decorator pipeline:
///
///   TelemetryDecorator
///     LoggingDecorator
///       DomainExceptionDecorator
///         OutputTransformDecorator      ← type boundary: TOutput → Result&lt;TOutput&gt;
///           [ ValidationDecorator ]     ← commands only
///             [ UoWDecorator      ]     ← commands only
///               handler
///
/// Commands (ICommandHandler) run validation and UoW.Save as inner steps.
/// Queries  (IQueryHandler)   skip both and go straight to OutputTransformDecorator.
/// All handlers return Result&lt;TOutput&gt; for a uniform BFF contract.
/// </summary>
public interface IHandlerInvoker
{
    Task<Result<TOutput>> InvokeAsync<TInput, TOutput>(
        IHandler<TInput, TOutput> handler,
        TInput input,
        CancellationToken ct);
}
