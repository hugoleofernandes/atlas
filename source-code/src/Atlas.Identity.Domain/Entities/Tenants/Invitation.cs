using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.ValueObjects;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants;

/// <summary>
/// Represents an invitation for a user to join a tenant with a specific role.
///
/// Invariants:
/// - Invitation cannot be used more than once.
/// - Invitation cannot be used after expiration.
/// - TenantRoleId references a valid TenantRole within the same tenant.
/// </summary>
public sealed class Invitation : AuditableEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid TenantId { get; private set; }

    public Email Email { get; private set; } = default!;

    public Guid TenantRoleId { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public bool IsUsed { get; private set; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public bool IsActive => !IsUsed && !IsExpired;

    private Invitation() { }

    internal Invitation(Guid tenantId, Email email, Guid tenantRoleId, InvitationTtl ttl)
    {
        TenantId = tenantId;
        Email = email;
        TenantRoleId = tenantRoleId;
        ExpiresAt = DateTime.UtcNow.Add(ttl.Value);
    }

    public void Use()
    {
        if (IsUsed)
            throw new InvitationAlreadyUsedException(Email.Value);

        if (IsExpired)
            throw new InvitationExpiredException(Email.Value);

        IsUsed = true;
    }
}
