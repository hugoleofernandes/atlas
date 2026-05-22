using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Idempotency;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers.Decorators;

/// <summary>
/// Idempotency guard for command handlers.
/// Sits as the outermost step in the IHandler chain inside CommandHandlerInvoker,
/// so a duplicate message short-circuits before validation, UoW, and execution.
///
/// Opt-in: only handlers that implement <see cref="IIdempotentHandler"/> are guarded.
/// <see cref="IIdempotencyContext"/> is populated externally before the pipeline runs
/// (by OutboxMessageDispatcher for outbox-triggered commands).
///
/// When the guard fires, <c>default!</c> is returned.
/// For <c>TOutput = Unit</c> (the only valid use-case for idempotent commands)
/// this is equivalent to <c>Unit.Value</c> and is treated as success.
/// </summary>
internal sealed class IdempotencyDecorator<TInput, TOutput> : IHandler<TInput, TOutput>
{
    private readonly IHandler<TInput, TOutput> _inner;
    private readonly IServiceProvider          _serviceProvider;

    public IdempotencyDecorator(
        IHandler<TInput, TOutput> inner,
        IServiceProvider          serviceProvider)
    {
        _inner           = inner;
        _serviceProvider = serviceProvider;
    }

    public async Task<TOutput> ExecuteAsync(TInput input, CancellationToken ct)
    {
        if (_inner is IIdempotentHandler)
        {
            var idempotency = _serviceProvider.GetRequiredService<IIdempotencyService>();
            if (await idempotency.HasAlreadyProcessedAsync(ct))
                return default!;
        }

        return await _inner.ExecuteAsync(input, ct);
    }
}
