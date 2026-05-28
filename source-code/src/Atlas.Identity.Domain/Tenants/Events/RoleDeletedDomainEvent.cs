using Atlas.SharedKernel.Domain.Events;

namespace Atlas.Identity.Domain.Tenants.Events;

public sealed class RoleDeletedDomainEvent : IDomainEvent
{
    public Guid TenantId { get; }
    public Guid RoleId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public RoleDeletedDomainEvent(Guid tenantId, Guid roleId)
    {
        TenantId = tenantId;
        RoleId = roleId;
    }
}
