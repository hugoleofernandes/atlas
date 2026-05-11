using Atlas.SharedKernel.Domain;
using System.Text.Json;

namespace Atlas.SharedKernel.Application.IntegrationEvents;

public sealed class OutboxMessageFactory : IOutboxMessageFactory
{
    public OutboxMessageFactory()
    {
    }

    public OutboxMessage Create(IDomainEvent domainEvent, OutboxEventDefinition outboxEventDefinition)
    {
        return new OutboxMessage(
            name: outboxEventDefinition.Name,
            type: nameof(outboxEventDefinition.Type),
            payload: JsonSerializer.Serialize(domainEvent),
            tenantId: Guid.Empty,// Get<Guid?>(e, "TenantId"),
            userId: Guid.Empty, // Get<Guid?>(e, "UserId"),
            correlationId: null,
            module: outboxEventDefinition.Module
        );
    }
}