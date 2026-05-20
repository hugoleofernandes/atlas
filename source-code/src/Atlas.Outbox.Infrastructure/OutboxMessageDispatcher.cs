using System.Reflection;
using System.Text.Json;
using Atlas.Outbox.Application.OutboxMessages;
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

        var handleMethod = handlerType.GetMethod(nameof(IIntegrationEventHandler<object>.HandleAsync))!;

        foreach (var handler in handlers)
            await (Task)handleMethod.Invoke(handler, [@event, ct])!;
    }
}
