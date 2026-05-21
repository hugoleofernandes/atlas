using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Idempotency;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.BuildingBlocks.Application.HandlerInvokers.Decorators;

/// <summary>
/// Idempotency guard for integration event handlers.
/// Sits between the handler and OutputTransformDecorator in the pipeline.
///
/// IIdempotencyContext is populated by OutboxMessageDispatcher before each handler
/// invocation — this decorator only reads from it via IIdempotencyService.
///
/// Guard runs only for handlers that implement IIdempotentHandler.
/// A skip returns Unit.Value — treated as success (already processed correctly).
/// </summary>
internal sealed class IntegrationIdempotencyDecorator<TEvent> : IHandler<TEvent, Unit>
{
    private readonly IHandler<TEvent, Unit> _inner;
    private readonly IServiceProvider       _serviceProvider;

    public IntegrationIdempotencyDecorator(
        IHandler<TEvent, Unit> inner,
        IServiceProvider       serviceProvider)
    {
        _inner           = inner;
        _serviceProvider = serviceProvider;
    }

    public async Task<Unit> ExecuteAsync(TEvent input, CancellationToken ct)
    {
        // IIdempotentHandler opt-in: skip if the (IdempotencyKey, HandlerName) pair
        // was already recorded. Context was set by dispatcher before the pipeline ran.
        if (_inner is IIdempotentHandler)
        {
            var idempotency = _serviceProvider.GetRequiredService<IIdempotencyService>();
            if (await idempotency.HasAlreadyProcessedAsync(ct))
                return Unit.Value;
        }

        return await _inner.ExecuteAsync(input, ct);
    }
}
