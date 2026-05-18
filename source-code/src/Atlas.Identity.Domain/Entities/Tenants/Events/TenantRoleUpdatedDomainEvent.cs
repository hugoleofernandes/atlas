using Atlas.SharedKernel.Domain.Events;

namespace Atlas.Identity.Domain.Entities.Tenants.Events;

public sealed class TenantRoleUpdatedDomainEvent : IDomainEvent
{
    public Guid TenantId { get; }
    public Guid RoleId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public TenantRoleUpdatedDomainEvent(Guid tenantId, Guid roleId)
    {
        TenantId = tenantId;
        RoleId = roleId;
    }
}
