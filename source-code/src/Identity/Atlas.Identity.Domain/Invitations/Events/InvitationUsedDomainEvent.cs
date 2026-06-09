using Atlas.SharedKernel.Domain.Events;

namespace Atlas.Identity.Domain.Invitations.Events;

public sealed class InvitationUsedDomainEvent : IDomainEvent
{
    public Guid TenantId { get; }
    public Guid UserId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;


    public string Email { get; }


    public InvitationUsedDomainEvent(Guid tenantId, Guid userId, string email)
    {
        TenantId = tenantId;
        UserId = userId;
        Email = email;
    }
}