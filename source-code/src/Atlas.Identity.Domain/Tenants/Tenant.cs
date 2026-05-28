using Atlas.Identity.Domain.Tenants.Events;
using Atlas.Identity.Domain.Tenants.Exceptions;
using Atlas.Identity.Domain.Tenants.Roles;
using Atlas.Identity.Domain.Tenants.Roles.Exceptions;
using Atlas.SharedKernel.Domain;
using Atlas.SharedKernel.Domain.Permissions;

namespace Atlas.Identity.Domain.Tenants;

/// <summary>
/// Purpose:
/// Represents an organizational boundary that owns roles and controls role/permission management.
/// User and Invitation lifecycle is managed by their own aggregate roots.
///
/// Invariants:
/// - A tenant cannot be inactive when performing domain operations.
/// - Role names must be unique within the tenant.
/// - Permission codes must exist in the valid codes set provided by IPermissionPolicy.
///
/// Boundaries:
/// - Does NOT own Users or Invitations (they are separate aggregate roots).
/// - Does NOT validate external identity providers.
/// - Does NOT send emails or notifications.
/// - Does NOT persist data (handled by repositories/UoW).
/// </summary>
public sealed class Tenant : AggregateRoot
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

    private readonly List<Role> _roles = [];
    public IReadOnlyCollection<Role> Roles => _roles;

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
    ///
    /// Parameters:
    /// - allCodes: all assignable codes across all modules (from IPermissionPolicy.All)
    /// - allIncludingSystemCodes: same plus system.root (from IPermissionPolicy.AllIncludingSystem)
    /// - memberPermissions: permission codes to assign to the default "member" role;
    ///   provided by the caller (seeder) so Identity domain does not need to know about other modules.
    /// </summary>
    public void SeedDefaultRoles(
        IReadOnlySet<string> allCodes,
        IReadOnlySet<string> allIncludingSystemCodes,
        IEnumerable<string> memberPermissions)
    {
        var root   = Role.Create(Id, "root",   allIncludingSystemCodes, allIncludingSystemCodes, isSystem: true, id: SystemRoleIds.Root);
        var admin  = Role.Create(Id, "admin",  allCodes,                allCodes,                isSystem: true, id: SystemRoleIds.Admin);
        var member = Role.Create(Id, "member", memberPermissions,       allCodes,                isSystem: true, id: SystemRoleIds.Member);

        _roles.Add(root);
        _roles.Add(admin);
        _roles.Add(member);
    }

    /// <summary>
    /// Creates a custom role for this tenant with the specified permissions.
    ///
    /// Invariants:
    /// - Tenant must be active.
    /// - Role name must be unique within this tenant.
    /// - All permission codes must exist in the provided validCodes set (from IPermissionPolicy.All).
    ///
    /// Emits: RoleCreatedDomainEvent
    /// </summary>
    public Role AddRole(string name, IEnumerable<string> permissionCodes, IReadOnlySet<string> validCodes)
    {
        EnsureActive();

        if (_roles.Any(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new RoleAlreadyExistsException(name);

        var role = Role.Create(Id, name, permissionCodes, validCodes);
        _roles.Add(role);
        AddDomainEvent(new RoleCreatedDomainEvent(Id, role.Id));
        return role;
    }

    /// <summary>
    /// Removes a role from the tenant.
    ///
    /// Behavior:
    /// - Hard delete when the role has no historical references (users or invitations).
    /// - Soft delete (IsActive = false) when historical references exist but no active ones.
    ///
    /// Invariants:
    /// - Tenant must be active.
    /// - System roles cannot be removed.
    /// - Role must have no active users assigned (checked by caller via IUserRepository).
    /// - Role must have no active pending invitations (checked by caller via IInvitationRepository).
    ///
    /// Parameters are pre-computed by the command handler to avoid loading unbounded collections.
    ///
    /// Emits: RoleDeletedDomainEvent (hard) or RoleDeactivatedDomainEvent (soft)
    /// </summary>
    public void RemoveRole(Guid roleId, bool hasActiveUsers, bool hasActiveInvitations, bool hasHistory)
    {
        EnsureActive();

        var role = _roles.FirstOrDefault(r => r.Id == roleId)
            ?? throw new RoleNotFoundException(roleId);

        if (role.IsSystem)
            throw new SystemRoleCannotBeModifiedException(role.Name);

        if (hasActiveUsers)
            throw new RoleInUseByUsersException(role.Name);

        if (hasActiveInvitations)
            throw new RoleInUseByInvitationsException(role.Name);

        if (hasHistory)
        {
            role.Deactivate();
            AddDomainEvent(new RoleDeactivatedDomainEvent(Id, roleId));
        }
        else
        {
            _roles.Remove(role);
            AddDomainEvent(new RoleDeletedDomainEvent(Id, roleId));
        }
    }

    /// <summary>
    /// Updates the name and permission set of an existing custom role.
    ///
    /// Invariants:
    /// - Tenant must be active.
    /// - Role must exist.
    /// - Role must not be a system role.
    /// - New name must be unique within the tenant (active and inactive roles included).
    /// - All permission codes must exist in the provided validCodes set (from IPermissionPolicy.All).
    ///
    /// Emits: RoleUpdatedDomainEvent
    /// </summary>
    public void UpdateRole(Guid roleId, string name, IEnumerable<string> permissionCodes, IReadOnlySet<string> validCodes)
    {
        EnsureActive();

        var role = _roles.FirstOrDefault(r => r.Id == roleId)
            ?? throw new RoleNotFoundException(roleId);

        if (role.IsSystem)
            throw new SystemRoleCannotBeModifiedException(role.Name);

        if (_roles.Any(r => r.Id != roleId && r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new RoleAlreadyExistsException(name);

        role.Rename(name);
        role.UpdatePermissions(permissionCodes, validCodes);
        AddDomainEvent(new RoleUpdatedDomainEvent(Id, roleId));
    }

    // =========================
    // INVITATION GUARD
    // =========================

    /// <summary>
    /// Validates that the tenant is active and the specified role exists.
    /// Must be called by the application layer before creating an Invitation.
    ///
    /// Throws:
    /// - TenantInactiveException if the tenant is inactive.
    /// - RoleNotFoundException if no role with roleId exists in this tenant.
    /// </summary>
    public void EnsureRoleExists(Guid roleId)
    {
        EnsureActive();

        if (!_roles.Any(r => r.Id == roleId))
            throw new RoleNotFoundException(roleId);
    }
}
