using Atlas.SharedKernel.Domain.Events;

namespace Atlas.SharedKernel.Application.OutboxMessages;

public interface IOutboxMessageFactory
{
    OutboxMessage Create<T>(T domainEvent, OutboxEventDefinition outboxEventDefinition)
        where T: IDomainEvent;
}
