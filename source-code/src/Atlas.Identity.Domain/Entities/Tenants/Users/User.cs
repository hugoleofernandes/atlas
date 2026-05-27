using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants.Users;

/// <summary>
/// Represents an authenticated user within a tenant.
///
/// Invariants:
/// - ExternalId uniquely identifies the user in the identity provider.
/// - A user can be deactivated but not deleted.
/// - Role assignment is managed via RoleId (resolved by the Tenant aggregate).
///
/// Design Decisions:
/// - Authentication is delegated to external providers (OIDC).
/// - The system does not manage passwords or credentials.
/// - Role is a FK to Role, which carries the actual permission set.
/// </summary>
public sealed class User : AuditableEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid TenantId { get; private set; }

    public ExternalId ExternalId { get; private set; } = default!;

    public Email Email { get; private set; } = default!;

    public Guid RoleId { get; private set; }

    public bool IsActive { get; private set; } = true;

    private User() { }

    public User(Guid tenantId, ExternalId externalId, Email email, Guid roleId)
    {
        TenantId = tenantId;
        ExternalId = externalId;
        Email = email;
        RoleId = roleId;
    }

    public void ChangeRole(Guid roleId)
    {
        RoleId = roleId;
    }

    public void Deactivate() => IsActive = false;
}
