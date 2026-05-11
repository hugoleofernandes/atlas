using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Application.IntegrationEvents;

public interface IIntegrationEventRegistry
{
    OutboxEventDefinition? Resolve(DomainEvent e, IOutboxMappings outboxMappings);

    IEnumerable<IntegrationEventMapping> ResolveAll(IEnumerable<DomainEvent> events, IOutboxMappings outboxMappings);
}
