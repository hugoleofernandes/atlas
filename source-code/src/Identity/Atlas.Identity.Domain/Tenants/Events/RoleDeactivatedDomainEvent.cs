using Atlas.SharedKernel.Domain.Events;

namespace Atlas.Identity.Domain.Tenants.Events;

public sealed class RoleDeactivatedDomainEvent : IDomainEvent
{
    public Guid TenantId { get; }
    public Guid RoleId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public RoleDeactivatedDomainEvent(Guid tenantId, Guid roleId)
    {
        TenantId = tenantId;
        RoleId = roleId;
    }
}
