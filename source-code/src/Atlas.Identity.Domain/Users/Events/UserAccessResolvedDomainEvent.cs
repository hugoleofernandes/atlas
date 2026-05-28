using Atlas.SharedKernel.Domain.Events;

namespace Atlas.Identity.Domain.Users.Events;

public sealed class UserAccessResolvedDomainEvent : IDomainEvent
{
    public Guid TenantId { get; }
    public Guid UserId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;


    public UserAccessResolvedDomainEvent(Guid tenantId, Guid userId)
    {
        TenantId = tenantId;
        UserId = userId;
    }
}