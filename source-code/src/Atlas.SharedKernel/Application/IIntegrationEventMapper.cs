using Atlas.SharedKernel.Domain;

namespace Atlas.SharedKernel.Application;

public interface IIntegrationEventMapper
{
    OutboxMessage Map(IDomainEvent e, IRequestContext ctx);
}
