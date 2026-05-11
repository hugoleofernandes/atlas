using Atlas.SharedKernel.Domain;

namespace Atlas.SharedKernel.Application.IntegrationEvents;

public interface IOutboxMessageFactory
{
    OutboxMessage Create(IDomainEvent domainEvent, OutboxEventDefinition outboxEventDefinition);
}
