using Atlas.Identity.Domain.Entities.Tenants.Events;
using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.ValueObjects;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants;

/// <summary>
/// Purpose:
/// Represents an organizational boundary that owns users, invitations, and roles.
/// Controls access resolution, invitation lifecycle, and role/permission management.
///
/// Invariants:
/// - A tenant cannot be inactive when performing domain operations.
/// - A tenant cannot have two users with the same email.
/// - A tenant cannot have two active invitations for the same email.
/// - A user must always be created from a valid and active invitation.
/// - Role names must be unique within the tenant.
/// - Permission codes must exist in PermissionCatalog.All.
///
/// Boundaries:
/// - Does NOT validate external identity providers.
/// - Does NOT send emails or notifications.
/// - Does NOT persist data (handled by repositories/UoW).
/// </summary>
public sealed class Tenant : AggregateRootBase
{
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

    private readonly List<User> _users = [];
    public IReadOnlyCollection<User> Users => _users;

    private readonly List<Invitation> _invitations = [];
    public IReadOnlyCollection<Invitation> Invitations => _invitations;

    private readonly List<TenantRole> _roles = [];
    public IReadOnlyCollection<TenantRole> Roles => _roles;

    private Tenant() { }

    public Tenant(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new TenantNameRequiredException();

        Name = name.ToLowerInvariant();
    }

    private void EnsureActive()
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

    // =========================
    // ROLE MANAGEMENT
    // =========================

    /// <summary>
    /// Seeds the default system roles for this tenant.
    /// Called once when a tenant is first created.
    /// System roles cannot be modified or deleted.
    /// </summary>
    public void SeedDefaultRoles()
    {
        var admin = TenantRole.Create(Id, "admin", PermissionCatalog.All, isSystem: true);
        var member = TenantRole.Create(Id, "member",
        [
            PermissionCatalog.Staff.Read,
            PermissionCatalog.Staff.Create,
            PermissionCatalog.Staff.Update,
        ], isSystem: true);
        var viewer = TenantRole.Create(Id, "viewer",
        [
            PermissionCatalog.Staff.Read,
        ], isSystem: true);

        _roles.Add(admin);
        _roles.Add(member);
        _roles.Add(viewer);
    }

    /// <summary>
    /// Creates a custom role for this tenant with the specified permissions.
    ///
    /// Invariants:
    /// - Tenant must be active.
    /// - Role name must be unique within this tenant.
    /// - All permission codes must exist in PermissionCatalog.All.
    ///
    /// Emits: TenantRoleCreatedDomainEvent
    /// </summary>
    public TenantRole AddCustomRole(string name, IEnumerable<string> permissionCodes)
    {
        EnsureActive();

        if (_roles.Any(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new RoleAlreadyExistsException(name);

        var role = TenantRole.Create(Id, name, permissionCodes);
        _roles.Add(role);
        AddDomainEvent(new TenantRoleCreatedDomainEvent(Id, role.Id));
        return role;
    }

    /// <summary>
    /// Updates the permission set of an existing custom role.
    ///
    /// Invariants:
    /// - Tenant must be active.
    /// - Role must exist.
    /// - Role must not be a system role.
    /// - All permission codes must exist in PermissionCatalog.All.
    ///
    /// Emits: TenantRoleUpdatedDomainEvent
    /// </summary>
    public void UpdateRolePermissions(Guid roleId, IEnumerable<string> permissionCodes)
    {
        EnsureActive();

        var role = _roles.FirstOrDefault(r => r.Id == roleId)
            ?? throw new RoleNotFoundException(roleId);

        role.UpdatePermissions(permissionCodes);
        AddDomainEvent(new TenantRoleUpdatedDomainEvent(Id, roleId));
    }

    // =========================
    // DOMAIN BEHAVIOR
    // =========================

    /// <summary>
    /// Creates an invitation for a user within the tenant.
    ///
    /// Invariants:
    /// - Tenant must be active.
    /// - No two active invitations may exist for the same email.
    /// - Invitations cannot be created for existing users.
    /// - The specified role must exist in this tenant.
    ///
    /// Emits: UserInvitedDomainEvent
    /// </summary>
    public Invitation InviteUser(Email email, Guid roleId, InvitationTtl ttl)
    {
        EnsureActive();

        if (!_roles.Any(r => r.Id == roleId))
            throw new RoleNotFoundException(roleId);

        if (_users.Any(x => x.Email.Value == email.Value))
            throw new UserAlreadyExistsException(email.Value);

        var activeInvitation = _invitations
            .FirstOrDefault(x => x.Email.Value == email.Value && x.IsActive);

        if (activeInvitation is not null)
            throw new DuplicateInvitationException(email.Value);

        var invitation = new Invitation(Id, email, roleId, ttl);
        _invitations.Add(invitation);
        AddDomainEvent(new UserInvitedDomainEvent(Id, email.Value));

        return invitation;
    }

    /// <summary>
    /// Resolves access for a user within the tenant.
    ///
    /// Behavior:
    /// - Returns an existing active user.
    /// - Or creates a new user from a valid invitation.
    ///
    /// Invariants:
    /// - Tenant must be active.
    /// - A user must come from a valid and active invitation.
    /// - No two users may share the same email.
    ///
    /// Emits: InvitationUsedDomainEvent, UserCreatedFromInvitationDomainEvent, UserAccessResolvedDomainEvent
    /// </summary>
    public User ResolveAccess(ExternalId externalId, Email email)
    {
        EnsureActive();

        var existingUser = _users.FirstOrDefault(x => x.Email.Value == email.Value && x.IsActive);
        if (existingUser is not null)
        {
            if (existingUser.ExternalId.Value != externalId.Value)
                throw new UserAlreadyExistsException(email.Value);

            AddDomainEvent(new UserAccessResolvedDomainEvent(Id, existingUser.Id));
            return existingUser;
        }

        var invitation = _invitations.FirstOrDefault(x => x.Email.Value == email.Value)
            ?? throw new InvitationNotFoundException(email.Value);

        if (!invitation.IsActive)
            throw new InvitationExpiredException(email.Value);

        invitation.Use();
        AddDomainEvent(new InvitationUsedDomainEvent(Id, invitation.Id, invitation.Email.Value));

        var user = CreateUserFromInvitation(invitation, externalId);

        var roleName = _roles.FirstOrDefault(r => r.Id == user.TenantRoleId)?.Name ?? string.Empty;
        AddDomainEvent(new UserCreatedFromInvitationDomainEvent(Id, user.Id, user.Email.Value, roleName));
        AddDomainEvent(new UserAccessResolvedDomainEvent(Id, user.Id));

        return user;
    }

    private User CreateUserFromInvitation(Invitation invitation, ExternalId externalId)
    {
        var user = new User(Id, externalId, invitation.Email, invitation.TenantRoleId);
        _users.Add(user);
        return user;
    }
}
