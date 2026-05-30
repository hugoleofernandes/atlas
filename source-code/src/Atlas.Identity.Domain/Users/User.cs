using Atlas.Identity.Domain.Invitations;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Domain.Users.Events;
using Atlas.Identity.Domain.Users.Exceptions;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Users;

/// <summary>
/// Represents an authenticated user within a tenant.
///
/// Invariants:
/// - ExternalId uniquely identifies the user in the identity provider.
/// - A user can be deactivated but not deleted.
/// - Role assignment is managed via RoleId (references a Role within the same tenant).
///
/// Design Decisions:
/// - Authentication is delegated to external providers (OIDC).
/// - The system does not manage passwords or credentials.
/// </summary>
public sealed class User : AggregateRoot, IMultiTenantEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid TenantId { get; private set; }

    public ExternalId ExternalId { get; private set; } = default!;

    public Email Email { get; private set; } = default!;

    public Guid RoleId { get; private set; }

    public bool IsActive { get; private set; } = true;

    void IMultiTenantEntity.SetTenantId(Guid tenantId) => TenantId = tenantId;

    private User() { }

    internal User(Guid tenantId, ExternalId externalId, Email email, Guid roleId)
    {
        TenantId = tenantId;
        ExternalId = externalId;
        Email = email;
        RoleId = roleId;
    }

    /// <summary>
    /// Creates a new user from a valid invitation.
    /// The invitation must already have been validated and marked as used before calling this.
    /// Emits UserCreatedFromInvitationDomainEvent and UserAccessResolvedDomainEvent.
    /// </summary>
    public static User CreateFromInvitation(Invitation invitation, ExternalId externalId, string roleName)
    {
        var user = new User(invitation.TenantId, externalId, invitation.Email, invitation.RoleId);
        user.AddDomainEvent(
            new UserCreatedFromInvitationDomainEvent(invitation.TenantId, user.Id, invitation.Email.Value, roleName)
        );
        user.AddDomainEvent(new UserAccessResolvedDomainEvent(invitation.TenantId, user.Id));
        return user;
    }

    /// <summary>
    /// Dev-only: creates an in-memory, non-persisted user for development login bypass.
    /// Uses a deterministic ExternalId derived from the email so that
    /// <see cref="ResolveExistingAccess"/> passes on the same request.
    /// Never call this outside of the Development environment.
    /// </summary>
    public static User CreateForDev(Guid tenantId, Email email, Guid roleId)
    {
        var externalId = ExternalId.Create($"dev:{email.Value}");
        return new User(tenantId, externalId, email, roleId);
    }

    /// <summary>
    /// Resolves access for an already-existing user.
    /// Validates that the ExternalId from the token matches the stored one (security check).
    /// Emits UserAccessResolvedDomainEvent.
    /// </summary>
    public void ResolveExistingAccess(ExternalId externalId)
    {
        if (ExternalId.Value != externalId.Value)
            throw new UserIdentityMismatchException(Email.Value);

        AddDomainEvent(new UserAccessResolvedDomainEvent(TenantId, Id));
    }

    public void ChangeRole(Guid roleId)
    {
        RoleId = roleId;
    }

    public void Deactivate() => IsActive = false;
}
