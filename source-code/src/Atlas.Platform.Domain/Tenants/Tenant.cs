using Atlas.Platform.Domain.Tenants.Events;
using Atlas.Platform.Domain.Tenants.Exceptions;
using Atlas.SharedKernel.Domain;
using Atlas.SharedKernel.Platform.Domain;

namespace Atlas.Platform.Domain.Tenants;

/// <summary>
/// Represents an organizational boundary that scopes users, invitations, and roles.
///
/// Invariants:
/// - A tenant cannot be inactive when performing domain operations.
///
/// Boundaries:
/// - Does NOT own Roles, Users, or Invitations â€” they are separate aggregate roots.
/// - Does NOT validate external identity providers.
/// - Does NOT send emails or notifications.
/// - Does NOT persist data (handled by repositories/UoW).
/// </summary>
public sealed class Tenant : AggregateRoot, INotMultiTenant, IAuditableAggregate
{
    public Guid EntityTypeId => PlatformEntityTypes.RootTenantId;

    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Name of the Microsoft Entra ID (Azure AD) tenant associated with this tenant.
    /// This value comes from the authentication context and identifies the
    /// Entra ID directory (e.g., "tenant01" or "tenant01.onmicrosoft.com").
    /// Not intended to be a user-friendly display name.
    /// </summary>
    public string Name { get; private set; } = default!;

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Tenant() { }

    public Tenant(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new TenantNameRequiredException();

        Name = name.ToLowerInvariant();
    }

    public void EnsureActive()
    {
        if (!IsActive)
            throw new TenantInactiveException();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        AddDomainEvent(new TenantDeactivatedDomainEvent(Id));
    }
}
