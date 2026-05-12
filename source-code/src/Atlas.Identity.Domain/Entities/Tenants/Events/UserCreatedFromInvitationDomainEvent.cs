using Atlas.SharedKernel.Domain.Events;

namespace Atlas.Identity.Domain.Entities.Tenants.Events;

public sealed class UserCreatedFromInvitationDomainEvent : IDomainEvent
{
    public Guid TenantId { get; }
    public Guid UserId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

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