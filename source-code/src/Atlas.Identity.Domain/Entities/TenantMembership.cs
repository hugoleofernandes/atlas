namespace Atlas.Identity.Domain.Entities;

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
/// - IdentityUserId is nullable to allow pre-registration invitations.
///
/// Boundaries:
/// - Does not validate authentication or identity provider data.
/// - Does not enforce tenant-level rules (handled by Tenant).
/// </summary>
public sealed class TenantMembership
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid TenantId { get; private set; }

    public Guid? IdentityUserId { get; private set; }  // 🔹 AGORA NULLABLE

    public string Email { get; private set; }

    public string Role { get; private set; }

    public bool IsActive { get; private set; } = true;

    private TenantMembership() { }

    internal TenantMembership(Guid tenantId, string email, string role)
    {
        TenantId = tenantId;
        Email = email.ToLowerInvariant();
        Role = role;
    }

    public void BindIdentityUser(Guid identityUserId)
    {
        if (IdentityUserId.HasValue)
            return;

        IdentityUserId = identityUserId;
    }

    public void Deactivate() => IsActive = false;
}