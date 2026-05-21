using Atlas.BuildingBlocks.Application.HandlerInvokers.Interfaces;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers;

/// <summary>
/// Routes handlers to the correct invoker based on their type:
///
///   ICommandHandler          → CommandHandlerInvoker    (validation + persist + observability)
///   IIntegrationEventHandler → IntegrationEventHandlerInvoker (idempotency + observability)
///   IQueryHandler            → QueryHandlerInvoker      (observability only)
///
/// All three invokers follow the same Result&lt;TOutput&gt; family contract.
/// The public IHandlerInvoker contract is unchanged — callers (controllers, dispatcher, tests)
/// do not need to know which invoker runs underneath.
///
/// Routing is strict: a handler must implement exactly one of the three contracts.
/// Ambiguity (multiple contracts) or unknown types throw at the point of invocation,
/// surfacing misconfiguration as early as possible.
/// </summary>
public sealed class HandlerInvoker : IHandlerInvoker
{
    private readonly CommandHandlerInvoker          _commandInvoker;
    private readonly QueryHandlerInvoker            _queryInvoker;
    private readonly IntegrationEventHandlerInvoker _integrationInvoker;

    public HandlerInvoker(
        ILoggerFactory   loggerFactory,
        IServiceProvider serviceProvider,
        IRequestContext  requestContext)
    {
        _commandInvoker     = new CommandHandlerInvoker(loggerFactory, serviceProvider, requestContext);
        _queryInvoker       = new QueryHandlerInvoker(loggerFactory, requestContext);
        _integrationInvoker = new IntegrationEventHandlerInvoker(loggerFactory, serviceProvider, requestContext);
    }

    /// <inheritdoc/>
    public Task<Result<TOutput>> InvokeAsync<TInput, TOutput>(
        IHandler<TInput, TOutput> handler,
        TInput                    input,
        CancellationToken         ct)
    {
        var isCommand     = handler is ICommandHandler<TInput, TOutput>;
        var isQuery       = handler is IQueryHandler<TInput, TOutput>;
        var isIntegration = handler is IIntegrationEventHandler;

        var matchCount = (isCommand ? 1 : 0) + (isQuery ? 1 : 0) + (isIntegration ? 1 : 0);

        if (matchCount == 0)
            throw new InvalidOperationException(
                $"'{handler.GetType().Name}' does not implement a recognised handler contract. " +
                $"Expected ICommandHandler<,>, IQueryHandler<,>, or IIntegrationEventHandler<>.");

        if (matchCount > 1)
        {
            var matched = string.Join(", ", new[]
            {
                isCommand     ? "ICommandHandler"          : null,
                isQuery       ? "IQueryHandler"            : null,
                isIntegration ? "IIntegrationEventHandler" : null,
            }.Where(x => x is not null));

            throw new InvalidOperationException(
                $"'{handler.GetType().Name}' implements more than one handler contract ({matched}). " +
                $"A handler must implement exactly one of: ICommandHandler<,>, IQueryHandler<,>, IIntegrationEventHandler<>.");
        }

        if (isCommand)
            return _commandInvoker.InvokeAsync((ICommandHandler<TInput, TOutput>)handler, input, ct);

        if (isIntegration)
        {
            var unitHandler = (IHandler<TInput, Unit>)(object)handler;
            var unitTask    = _integrationInvoker.InvokeAsync(unitHandler, input, ct);
            return (Task<Result<TOutput>>)(object)unitTask;
        }

        return _queryInvoker.InvokeAsync(handler, input, ct);
    }
}
