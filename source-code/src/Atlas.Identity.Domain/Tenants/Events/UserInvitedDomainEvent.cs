using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Tenants.Events;

public sealed class UserInvitedDomainEvent : DomainEvent
{
    public Guid TenantId { get; }
    public string Email { get; }
    public string Role { get; }

    public UserInvitedDomainEvent(Guid tenantId, string email, string role)
    {
        TenantId = tenantId;
        Email = email;
        Role = role;
    }
}