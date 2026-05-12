using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Application.IntegrationEvents;

public interface IIntegrationEventRegistry
{
    OutboxEventDefinition? Resolve(IDomainEvent e, IOutboxMappings outboxMappings);

    IEnumerable<IntegrationEventMapping> ResolveAll(IEnumerable<IDomainEvent> events, IOutboxMappings outboxMappings);
}
