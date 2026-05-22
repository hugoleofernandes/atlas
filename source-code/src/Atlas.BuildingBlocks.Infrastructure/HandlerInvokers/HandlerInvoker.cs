using Atlas.BuildingBlocks.Application.HandlerInvokers.Interfaces;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.Extensions.Logging;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers;

/// <summary>
/// Routes handlers to the correct invoker based on their type:
///
///   IQueryHandler   → QueryHandlerInvoker   (observability only — no UoW, no validation)
///   everything else → CommandHandlerInvoker (full pipeline — idempotency, validation, UoW,
///                                            observability; all three inner decorators are
///                                            safe no-ops when not applicable)
///
/// "Everything else" covers:
///   • ICommandHandler&lt;,&gt; — exposes UnitOfWork; PersistDbDecorator calls SaveChangesAsync.
///   • IIntegrationEventHandler adapters — thin mappers that delegate to a command handler
///     via a nested IHandlerInvoker call; PersistDbDecorator is a no-op (NullUnitOfWork).
///
/// All invokers return Result&lt;TOutput&gt; — uniform contract for controllers and the dispatcher.
/// </summary>
public sealed class HandlerInvoker : IHandlerInvoker
{
    private readonly CommandHandlerInvoker _commandInvoker;
    private readonly QueryHandlerInvoker   _queryInvoker;

    public HandlerInvoker(
        ILoggerFactory   loggerFactory,
        IServiceProvider serviceProvider,
        IRequestContext  requestContext)
    {
        _commandInvoker = new CommandHandlerInvoker(loggerFactory, serviceProvider, requestContext);
        _queryInvoker   = new QueryHandlerInvoker(loggerFactory, requestContext);
    }

    /// <inheritdoc/>
    public Task<Result<TOutput>> InvokeAsync<TInput, TOutput>(
        IHandler<TInput, TOutput> handler,
        TInput                    input,
        CancellationToken         ct)
    {
        // Queries get a lighter pipeline — skip UoW, validation, and idempotency.
        if (handler is IQueryHandler<TInput, TOutput>)
            return _queryInvoker.InvokeAsync(handler, input, ct);

        // Commands and integration-event adapters share the same full pipeline.
        // CommandHandlerInvoker derives UnitOfWork from ICommandHandler if available,
        // falling back to NullUnitOfWork for adapters.
        return _commandInvoker.InvokeAsync(handler, input, ct);
    }
}
