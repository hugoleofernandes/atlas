using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.SharedKernel.Domain.Events;

namespace Atlas.BuildingBlocks.Application.OutboxMessages;

public sealed class OutboxMessageBuilder : IOutboxMessageBuilder
{
    private readonly IEnumerable<IIntegrationEventMapper> _integrationEventMappers;

    public OutboxMessageBuilder(IEnumerable<IIntegrationEventMapper> integrationEventMappers)
    {
        _integrationEventMappers = integrationEventMappers;
    }

    public IEnumerable<OutboxMessage> BuildFromIntegrationEvents(IEnumerable<IDomainEvent> domainEvents, IEnumerable<IIntegrationEventMapper> integrationEventMappers)
    {
        var map = _integrationEventMappers.ToDictionary(x => x.DomainEventType);

        foreach (var domainEvent in domainEvents)
        {
            var domainEventType = domainEvent.GetType();

            if (!map.TryGetValue(domainEventType, out var mapper))
                continue;

            yield return mapper.Map(domainEvent);
        }
    }
}