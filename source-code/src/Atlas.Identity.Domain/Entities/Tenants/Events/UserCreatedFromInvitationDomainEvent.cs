using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Events;

public sealed class UserCreatedFromInvitationDomainEvent : DomainEvent, IIntegrationEvent
{
    public Guid TenantId { get; }
    public Guid UserId { get; }
    public string Email { get; }
    public string Role { get; }

    string IIntegrationEvent.EventName => "tenant.user-created-from-invitation";

    string IIntegrationEvent.Module => "identity";

    public UserCreatedFromInvitationDomainEvent(Guid tenantId, Guid userId, string email, string role)
    {
        TenantId = tenantId;
        UserId = userId;
        Email = email;
        Role = role;
    }
}