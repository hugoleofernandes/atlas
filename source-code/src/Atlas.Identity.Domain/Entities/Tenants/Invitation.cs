using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.ValueObjects;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants;

/// <summary>
/// Represents an invitation for a user to join a tenant.
///
/// Invariants:
/// - Invitation cannot be used more than once.
/// - Invitation cannot be used after expiration.
/// - Email, Role and TTL are validated by their respective value objects.
/// 
/// Purpose:
/// - Controls the lifecycle of an invitation.
/// - Ensures correct usage and expiration behavior.
/// </summary>
public sealed class Invitation : IEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid TenantId { get; private set; }

    public Email Email { get; private set; }

    public Role Role { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; private set; }

    public bool IsUsed { get; private set; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public bool IsActive => !IsUsed && !IsExpired;

    private Invitation() { }

    internal Invitation(Guid tenantId, Email email, Role role, InvitationTtl ttl)
    {
        TenantId = tenantId;
        Email = email;
        Role = role;
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
