using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Idempotency;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers.Decorators;

/// <summary>
/// Idempotency guard for command handlers.
/// Sits outside <c>PersistDbDecorator</c> and <c>ValidationDecorator</c> in the
/// <c>CommandHandlerInvoker</c> pipeline so a duplicate message short-circuits before
/// validation, UoW, and execution.
///
/// Only injected when the handler implements <see cref="IIdempotentHandler"/> —
/// that check lives in <c>CommandHandlerInvoker</c>, before wrapping starts.
/// This decorator always runs the guard unconditionally; it has no type-check of its own.
///
/// <see cref="IIdempotencyContext"/> is populated externally before the pipeline runs
/// (by <c>OutboxMessageDispatcher</c> for outbox-triggered commands).
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
        var idempotency = _serviceProvider.GetRequiredService<IIdempotencyService>();
        if (await idempotency.HasAlreadyProcessedAsync(ct))
            return default!;

        return await _inner.ExecuteAsync(input, ct);
    }
}
