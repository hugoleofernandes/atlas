using Atlas.BuildingBlocks.Application.HandlerInvokers.Decorators;
using Atlas.BuildingBlocks.Application.HandlerInvokers.Interfaces;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Idempotency;
using Microsoft.Extensions.Logging;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers;

/// <summary>
/// Executes any side-effecting handler through the full decorator pipeline.
/// Used for both application command handlers and integration-event adapters —
/// the "command-specific" decorators are safe no-ops when not applicable:
///
///   IdempotencyDecorator  — only injected when handler implements IIdempotentHandler
///   ValidationDecorator   — skips if no IValidator&lt;TInput&gt; is registered
///   PersistDbDecorator    — calls SaveChangesAsync, which is a no-op for NullUnitOfWork
///
/// Full pipeline (innermost → outermost):
///
///   handler
///     IdempotencyDecorator  ← deduplicates retries (IIdempotentHandler opt-in)
///       ValidationDecorator ← FluentValidation (IValidator opt-in)
///         PersistDbDecorator ← UnitOfWork.SaveChangesAsync (NullUnitOfWork for adapters)
///           OutputTransformDecorator
///             DomainExceptionDecorator
///               LoggingDecorator
///                 TelemetryDecorator
/// </summary>
internal sealed class CommandHandlerInvoker
{
    private readonly ILoggerFactory   _loggerFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRequestContext  _requestContext;

    public CommandHandlerInvoker(
        ILoggerFactory   loggerFactory,
        IServiceProvider serviceProvider,
        IRequestContext  requestContext)
    {
        _loggerFactory   = loggerFactory;
        _serviceProvider = serviceProvider;
        _requestContext  = requestContext;
    }

    public Task<Result<TOutput>> InvokeAsync<TInput, TOutput>(
        IHandler<TInput, TOutput> handler,
        TInput                    input,
        CancellationToken         ct)
    {
        var name = handler.GetType().Name;

        // ICommandHandler exposes its own UnitOfWork; everything else (adapters, etc.)
        // gets NullUnitOfWork so PersistDbDecorator runs safely as a no-op.
        var unitOfWork = (handler as ICommandHandler<TInput, TOutput>)?.UnitOfWork
                         ?? NullUnitOfWork.Instance;

        // Check before wrapping — once the handler is wrapped in other decorators,
        // the type-check inside IdempotencyDecorator would test the wrong object.
        bool isIdempotent = handler is IIdempotentHandler;

        IHandler<TInput, TOutput> handlerPipeline = handler;
        IResultPipelineStep<TInput, TOutput> pipeline;

        // ── Side-effect block (no-ops when not applicable) ─────────────────
        handlerPipeline = new ValidationDecorator<TInput, TOutput>(handlerPipeline, _serviceProvider);
        handlerPipeline = new PersistDbDecorator<TInput, TOutput>(handlerPipeline, unitOfWork);
        if (isIdempotent)
            handlerPipeline = new IdempotencyDecorator<TInput, TOutput>(handlerPipeline, _serviceProvider);
        // ──────────────────────────────────────────────────────────────────

        // ── Observability block ────────────────────────────────────────────
        pipeline = new OutputTransformDecorator<TInput, TOutput>(handlerPipeline);
        pipeline = new DomainExceptionDecorator<TInput, TOutput>(pipeline);
        pipeline = new LoggingDecorator<TInput, TOutput>(pipeline, _loggerFactory, handler.GetType(), name, layer: "handler");
        pipeline = new TelemetryDecorator<TInput, TOutput>(pipeline, name, layer: "handler", _requestContext.CorrelationId);
        // ──────────────────────────────────────────────────────────────────

        return pipeline.ExecuteAsync(input, ct);
    }
}
