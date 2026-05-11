using Atlas.SharedKernel.Domain.Events;

namespace Atlas.Identity.Domain.Entities.Tenants.Events;

/// <summary>
/// Represents the business fact that a tenant has been deactivated.
///
/// Meaning:
/// - The tenant can no longer perform domain operations.
/// - All invariants that require an active tenant are now blocked.
///
/// When emitted:
/// - Immediately after the tenant transitions from active to inactive.
///
/// Invariants communicated:
/// - A tenant must be active to execute domain behavior.
/// </summary>
public sealed class TenantDeactivatedDomainEvent : DomainEvent
{
    public Guid TenantId { get; }

    public TenantDeactivatedDomainEvent(Guid tenantId)
    {
        TenantId = tenantId;
    }
}
