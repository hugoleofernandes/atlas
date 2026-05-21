using Atlas.BuildingBlocks.Application.HandlerInvokers.Interfaces;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.Extensions.Logging;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers;

/// <summary>
/// Routes handlers to the correct invoker based on their type:
///
///   ICommandHandler → CommandHandlerInvoker (validation + persist + observability)
///   IQueryHandler   → QueryHandlerInvoker   (observability only)
///
/// The public IHandlerInvoker contract is unchanged — callers (controllers, tests)
/// do not need to know which invoker runs underneath.
/// </summary>
public sealed class HandlerInvoker : IHandlerInvoker
{
    private readonly CommandHandlerInvoker _commandInvoker;
    private readonly QueryHandlerInvoker _queryInvoker;

    public HandlerInvoker(
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider,
        IRequestContext requestContext)
    {
        _commandInvoker = new CommandHandlerInvoker(loggerFactory, serviceProvider, requestContext);
        _queryInvoker   = new QueryHandlerInvoker(loggerFactory, requestContext);
    }

    /// <inheritdoc/>
    public Task<Result<TOutput>> InvokeAsync<TInput, TOutput>(
        IHandler<TInput, TOutput> handler,
        TInput input,
        CancellationToken ct)
    {
        if (handler is ICommandHandler<TInput, TOutput> cmd)
            return _commandInvoker.InvokeAsync(cmd, input, ct);

        return _queryInvoker.InvokeAsync(handler, input, ct);
    }
}
