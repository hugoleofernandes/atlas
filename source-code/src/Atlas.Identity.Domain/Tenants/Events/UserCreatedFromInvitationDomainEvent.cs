using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Tenants.Events;

public sealed class UserCreatedFromInvitationDomainEvent : DomainEvent
{
    public Guid TenantId { get; }
    public Guid UserId { get; }
    public string Email { get; }
    public string Role { get; }

    public UserCreatedFromInvitationDomainEvent(Guid tenantId, Guid userId, string email, string role)
    {
        TenantId = tenantId;
        UserId = userId;
        Email = email;
        Role = role;
    }
}