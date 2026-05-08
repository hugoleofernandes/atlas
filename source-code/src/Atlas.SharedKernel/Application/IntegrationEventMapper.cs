using Atlas.SharedKernel.Domain;
using System.Text.Json;

namespace Atlas.SharedKernel.Application;

public sealed class IntegrationEventMapper
    : IIntegrationEventMapper
{
    public OutboxMessage? Map(
        IDomainEvent domainEvent,
        IRequestContext ctx)
    {
        if (domainEvent is not IIntegrationEvent integrationEvent)
            return null;

        return new OutboxMessage(
            name: integrationEvent.EventName,
            type: domainEvent.GetType().FullName!,
            payload: JsonSerializer.Serialize(domainEvent),
            tenantId: ctx.TenantId,
            userId: ctx.UserId,
            correlationId: string.Empty,
            module: integrationEvent.Module
        );
    }
}