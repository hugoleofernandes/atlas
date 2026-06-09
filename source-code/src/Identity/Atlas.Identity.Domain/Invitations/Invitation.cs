using Atlas.Identity.Domain.Invitations.Events;
using Atlas.Identity.Domain.Invitations.Exceptions;
using Atlas.Identity.Domain.Shared;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Invitations;

/// <summary>
/// Represents an invitation for a user to join a tenant with a specific role.
///
/// Invariants:
/// - Invitation cannot be used more than once.
/// - Invitation cannot be used after expiration.
/// - RoleId references a valid Role within the same tenant.
/// </summary>
public sealed class Invitation : AggregateRoot, IMultiTenantEntity, IAuditableAggregate
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid TenantId { get; private set; }

    public Email Email { get; private set; } = default!;

    public Guid RoleId { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public bool IsUsed { get; private set; }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public bool IsActive => IsUsed == false && IsExpired == false;

    void IMultiTenantEntity.SetTenantId(Guid tenantId) => TenantId = tenantId;

    private Invitation() { }

    private Invitation(Guid tenantId, Email email, Guid roleId, InvitationTtl ttl)
    {
        TenantId = tenantId;
        Email = email;
        RoleId = roleId;
        ExpiresAt = DateTime.UtcNow.Add(ttl.Value);
    }

    // =========================
    // FACTORY
    // =========================

    /// <summary>
    /// Creates a new invitation and emits UserInvitedDomainEvent.
    ///
    /// Pre-conditions (enforced by the caller before invoking this method):
    /// - The email must not belong to an existing active user.
    /// - No active invitation must already exist for this email.
    /// - The tenant must be active and the role must exist (validated via Tenant.EnsureRoleExists).
    ///
    /// Emits: UserInvitedDomainEvent
    /// </summary>
    public static Invitation Create(Guid tenantId, Email email, Guid roleId, InvitationTtl ttl)
    {
        var invitation = new Invitation(tenantId, email, roleId, ttl);
        invitation.AddDomainEvent(new UserInvitedDomainEvent(tenantId, email.Value));
        return invitation;
    }

    // =========================
    // BEHAVIOUR
    // =========================

    /// <summary>
    /// Marks the invitation as used.
    ///
    /// Invariants:
    /// - Cannot be used more than once.
    /// - Cannot be used after expiration.
    ///
    /// Emits: InvitationUsedDomainEvent
    /// </summary>
    public void Use()
    {
        if (IsUsed)
            throw new InvitationAlreadyUsedException(Email.Value);

        if (IsExpired)
            throw new InvitationExpiredException(Email.Value);

        IsUsed = true;
        AddDomainEvent(new InvitationUsedDomainEvent(TenantId, Id, Email.Value));
    }
}
