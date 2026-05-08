using Atlas.SharedKernel.Domain;
using System.Text.Json;

namespace Atlas.SharedKernel.Application;

public abstract class OutputBase
{
    public IAggregateRoot AggregateRoot { get; }

    public IEnumerable<IIntegrationEvent> IntegrationEvents => this.AggregateRoot.DomainEvents.IntegrationEvents();

    public TEvent? GetEvent<TEvent>()
        where TEvent : class, IIntegrationEvent
    {
        return IntegrationEvents
            .OfType<TEvent>()
            .FirstOrDefault();
    }

    public OutputBase(IAggregateRoot aggregateRoot)
    {
        AggregateRoot = aggregateRoot;
    }
}

public static class IntegrationEventExtensions
{
    public static OutboxMessage ToOutboxMessage(this IIntegrationEvent evt)
        => new OutboxMessage(
            name: evt.EventName,
            type: evt.GetType().FullName!,
            payload: JsonSerializer.Serialize(evt),
            tenantId: Guid.Empty, // evt.TenantId,
            userId: Guid.Empty, //evt.UserId,
            correlationId: string.Empty, // ctx.CorrelationId,
            module: evt.Module);
}

public static class DomainEventExtensions
{
    public static IEnumerable<IIntegrationEvent> IntegrationEvents(this IEnumerable<IDomainEvent> events)
        => events.OfType<IIntegrationEvent>();
}