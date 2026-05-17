using System.Text.Json;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.SharedKernel.Application.OutboxMessages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Atlas.OutboxWorker.Dispatching;

internal sealed class OutboxMessageDispatcher : IOutboxMessageDispatcher
{
    private readonly IIntegrationEventTypeResolver _typeResolver;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxMessageDispatcher> _logger;

    public OutboxMessageDispatcher(
        IIntegrationEventTypeResolver typeResolver,
        IServiceProvider serviceProvider,
        ILogger<OutboxMessageDispatcher> logger)
    {
        _typeResolver = typeResolver;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(OutboxMessage message, CancellationToken ct)
    {
        var eventType = _typeResolver.Resolve(message.Type)
            ?? throw new InvalidOperationException($"Integration event type '{message.Type}' not found.");

        var @event = JsonSerializer.Deserialize(message.Payload, eventType)
            ?? throw new InvalidOperationException($"Failed to deserialize payload for type '{message.Type}'.");

        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
        var handlers = _serviceProvider.GetServices(handlerType).ToList();

        if (handlers.Count == 0)
            throw new InvalidOperationException($"No handler registered for '{eventType.Name}'.");

        _logger.LogDebug(
            "Dispatching {EventType} to {HandlerCount} handler(s) (MessageId={MessageId})",
            eventType.Name, handlers.Count, message.Id);

        var handleMethod = handlerType.GetMethod(nameof(IIntegrationEventHandler<object>.HandleAsync))!;

        foreach (var handler in handlers)
        {
            await (Task)handleMethod.Invoke(handler, [@event, ct])!;
        }
    }
}
