using System.Reflection;
using System.Text.Json;
using Atlas.Outbox.Application.OutboxMessages;
using Atlas.SharedKernel.Application.Idempotency;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.SharedKernel.Application.OutboxMessages;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Infrastructure;

/// <summary>
/// Resolves the integration event type from the outbox message, deserializes the payload,
/// and invokes all registered IIntegrationEventHandler&lt;TEvent&gt; for that event type.
/// </summary>
internal sealed class OutboxMessageDispatcher : IOutboxMessageDispatcher
{
    private readonly IIntegrationEventTypeResolver _typeResolver;
    private readonly IServiceProvider _serviceProvider;

    public OutboxMessageDispatcher(
        IIntegrationEventTypeResolver typeResolver,
        IServiceProvider serviceProvider)
    {
        _typeResolver    = typeResolver;
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(OutboxMessage message, CancellationToken ct)
    {
        var eventType = _typeResolver.Resolve(message.Type)
            ?? throw new InvalidOperationException($"Integration event type '{message.Type}' not found.");

        var @event = JsonSerializer.Deserialize(message.Payload, eventType)
            ?? throw new InvalidOperationException($"Failed to deserialize payload for type '{message.Type}'.");

        var handlerType  = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var handlers     = _serviceProvider.GetServices(handlerType).ToList();

        if (handlers.Count == 0)
            throw new InvalidOperationException($"No handler registered for '{eventType.Name}'.");

        var handleMethod    = handlerType.GetMethod(nameof(IIntegrationEventHandler<object>.HandleAsync))!;
        var contextSetter   = _serviceProvider.GetRequiredService<IIdempotencyContextSetter>();

        // Run every handler regardless of individual failures.
        // A failure in one handler must not prevent the others from executing.
        // The message is retried as a whole if any handler fails — IIdempotencyService
        // protects handlers that already succeeded from re-executing business logic.
        var exceptions = new List<Exception>();

        foreach (var handler in handlers)
        {
            // Populate the scoped idempotency context for this specific handler invocation.
            // IdempotencyKey is stable across retries; HandlerName scopes it per handler.
            contextSetter.Set(message.IdempotencyKey, handler.GetType().Name);

            try
            {
                await (Task)handleMethod.Invoke(handler, [@event, ct])!;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count == 1)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exceptions[0]).Throw();

        if (exceptions.Count > 1)
            throw new AggregateException(
                $"{exceptions.Count} handler(s) failed for '{eventType.Name}'.", exceptions);
    }
}
