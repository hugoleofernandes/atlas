using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Tenants.Events;

public sealed class UserAccessResolvedDomainEvent : DomainEvent
{
    public Guid TenantId { get; }
    public Guid UserId { get; }

    public UserAccessResolvedDomainEvent(Guid tenantId, Guid userId)
    {
        TenantId = tenantId;
        UserId = userId;
    }
}