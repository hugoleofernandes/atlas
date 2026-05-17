using Atlas.Contracts.Tenants.IntegrationEvents;
using Atlas.Identity.Domain.Entities.Tenants.Events;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.SharedKernel.Domain.Events;

namespace Atlas.Identity.Application.Tenants.IntegrationEventMappers;

public sealed class UserCreatedFromInvitationMapper : IIntegrationEventMapper
{
    private readonly IOutboxMessageFactory _factory;

    public UserCreatedFromInvitationMapper(IOutboxMessageFactory factory)
    {
        _factory = factory;
    }

    public Type DomainEventType =>
        typeof(UserCreatedFromInvitationDomainEvent);

    public OutboxMessage Map(IDomainEvent domainEvent)
    {
        var e = (UserCreatedFromInvitationDomainEvent)domainEvent;

        var integrationEvent = new UserCreatedFromInvitationIntegrationEvent(
            e.TenantId,
            e.UserId,
            e.Email,
            e.Role
        );

        return _factory.Create(integrationEvent);
    }
}