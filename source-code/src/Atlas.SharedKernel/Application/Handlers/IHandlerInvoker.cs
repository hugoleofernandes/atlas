using Atlas.SharedKernel.Application.Commands;

namespace Atlas.SharedKernel.Application.Handlers;

/// <summary>
/// Executes any handler through an explicit decorator pipeline:
///
///   TelemetryDecorator
///     LoggingDecorator
///       DomainExceptionDecorator
///         OutputTransformDecorator      ← type boundary: TOutput → Result&lt;TOutput&gt;
///           [ IdempotencyDecorator ]    ← commands only (IIdempotentHandler opt-in)
///             [ ValidationDecorator ]   ← commands only (IValidator opt-in)
///               [ PersistDbDecorator ]  ← commands only (NullUnitOfWork for adapters)
///                 handler
///
/// Commands (ICommandHandler) run idempotency, validation and UoW.Save as inner steps.
/// Queries  (IQueryHandler)   skip all three and go straight to OutputTransformDecorator.
/// All handlers return Result&lt;TOutput&gt; for a uniform BFF contract.
/// </summary>
public interface IHandlerInvoker
{
    Task<Result<TOutput>> InvokeAsync<TInput, TOutput>(
        IHandler<TInput, TOutput> handler,
        TInput                    input,
        CancellationToken         ct);
}
