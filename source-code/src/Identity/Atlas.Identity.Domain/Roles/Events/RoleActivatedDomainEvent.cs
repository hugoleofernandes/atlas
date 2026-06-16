using Atlas.SharedKernel.Domain.Events;

namespace Atlas.Identity.Domain.Roles.Events;

public sealed class RoleActivatedDomainEvent : IDomainEvent
{
    public Guid TenantId { get; }
    public Guid RoleId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public RoleActivatedDomainEvent(Guid tenantId, Guid roleId)
    {
        TenantId = tenantId;
        RoleId = roleId;
    }
}
