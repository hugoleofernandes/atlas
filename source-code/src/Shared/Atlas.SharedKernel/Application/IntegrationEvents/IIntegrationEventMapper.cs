using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Application.IntegrationEvents;

public interface IIntegrationEventMapper
{
    Type DomainEventType { get; }

    OutboxMessage Map(IDomainEvent domainEvent);
}