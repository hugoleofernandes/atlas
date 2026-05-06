using Atlas.Identity.Domain.ValueObjects;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants;

/// <summary>
/// Represents an external authenticated user identity provided by an identity provider (OIDC).
///
/// Aggregate Root:
/// - Represents an authenticated user from an external identity provider.
///
/// Invariants:
/// - ExternalId uniquely identifies the user in the identity provider.
/// - A user can be deactivated but not deleted.
///
/// Design Decisions:
/// - Authentication is delegated to external providers (OIDC).
/// - The system does not manage passwords or credentials.
/// - IdentityUser is intentionally minimal and decoupled from domain-specific data.
///
/// Boundaries:
/// - Does not manage tenant membership (handled by Tenant aggregate).
/// - Does not store profile or business-related data.
/// </summary>
public sealed class User : IEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid TenantId { get; private set; }

    public ExternalId ExternalId { get; private set; }

    public Email Email { get; private set; }

    public Role Role { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private User() { }

    public User(Guid tenantId, ExternalId externalId, Email email, Role role)
    {
        TenantId = tenantId;
        ExternalId = externalId;
        Email = email;
        Role = role;
    }

    public void ChangeRole(Role role)
    {
        Role = role;
    }

    public void Deactivate() => IsActive = false;
}
