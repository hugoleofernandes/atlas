namespace Atlas.Identity.Domain.Tenants;

/// <summary>
/// Represents the association between a user (or invited email) and a tenant.
///
/// Entity (part of Tenant aggregate):
/// - Lifecycle is controlled by Tenant.
/// - Cannot exist independently.
///
/// Invariants:
/// - Email is normalized and unique per tenant (enforced by Tenant).
/// - A membership can be bound to at most one IdentityUser.
/// - A membership can exist without a bound IdentityUser (invitation phase).
///
/// Design Decisions:
/// - Supports invitation-first flow (email-based access before user creation).
/// - UserId is nullable to allow pre-registration invitations.
///
/// Boundaries:
/// - Does not validate authentication or identity provider data.
/// - Does not enforce tenant-level rules (handled by Tenant).
/// </summary>
public sealed class Membership
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid TenantId { get; private set; }

    public Guid? UserId { get; private set; }

    public string Email { get; private set; }

    public string Role { get; private set; }

    public bool IsActive { get; private set; } = true;

    private Membership() { }

    internal Membership(Guid tenantId, string email, string role)
    {
        TenantId = tenantId;
        Email = email.ToLowerInvariant();
        Role = role;
    }

    internal void BindUser(Guid userId)
    {
        if (UserId.HasValue)
            throw new InvalidOperationException("Membership already bound.");

        UserId = userId;
    }

    internal void Deactivate() => IsActive = false;
}