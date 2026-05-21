using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Application.OutboxMessages;

public interface IOutboxMessageBuilder
{
    IEnumerable<OutboxMessage> BuildFromIntegrationEvents(IEnumerable<IDomainEvent> domainEvents, IEnumerable<IIntegrationEventMapper> integrationEventMappers);
}
