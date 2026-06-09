using Atlas.SharedKernel.Domain.Events;

namespace Atlas.Identity.Domain.Invitations.Events;

public sealed class UserInvitedDomainEvent : IDomainEvent
{
    public Guid TenantId { get; }
    public string Email { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public UserInvitedDomainEvent(Guid tenantId, string email)
    {
        TenantId = tenantId;
        Email = email;
    }
}
