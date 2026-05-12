using Atlas.SharedKernel.Domain.Events;
using System.Text.Json;

namespace Atlas.SharedKernel.Application.OutboxMessages;

public sealed class OutboxMessageFactory : IOutboxMessageFactory
{
    public OutboxMessageFactory()
    {
    }

    public OutboxMessage Create<T>(T domainEvent, OutboxEventDefinition outboxEventDefinition)
        where T: IDomainEvent
    {
        return new OutboxMessage(
            name: outboxEventDefinition.Name,
            type: outboxEventDefinition.Type.ToString(),
            payload: JsonSerializer.Serialize(domainEvent),
            tenantId: Guid.Empty,// Get<Guid?>(e, "TenantId"),
            userId: Guid.Empty, // Get<Guid?>(e, "UserId"),
            correlationId: null,
            module: outboxEventDefinition.Module
        );
    }
}