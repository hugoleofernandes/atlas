using Atlas.SharedKernel.Domain.Events;

namespace Atlas.Identity.Domain.Entities.Tenants.Events;

public sealed class RoleUpdatedDomainEvent : IDomainEvent
{
    public Guid TenantId { get; }
    public Guid RoleId { get; }
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public RoleUpdatedDomainEvent(Guid tenantId, Guid roleId)
    {
        TenantId = tenantId;
        RoleId = roleId;
    }
}
