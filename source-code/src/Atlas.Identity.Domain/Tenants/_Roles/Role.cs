using Atlas.Identity.Domain.Tenants.Events;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.Identity.Domain.Tenants._Roles.Exceptions;
using Atlas.SharedKernel.Domain;
using Atlas.SharedKernel.Domain.Permissions;

namespace Atlas.Identity.Domain.Tenants._Roles;

/// <summary>
/// A named set of permissions scoped to a tenant.
///
/// Invariants:
/// - System roles (IsSystem=true) cannot have their permissions modified or be deleted.
/// - Every permission code must exist in the provided validCodes set (supplied by the caller from IPermissionPolicy).
/// - Role name uniqueness within the tenant is enforced by a unique index and pre-checked by command handlers.
///
/// Design: Role is an Aggregate Root — it has its own repository, lifecycle, and domain events.
/// The domain does not reference IPermissionPolicy directly — callers pass the valid set as a parameter,
/// keeping the domain pure and enabling modular permission registration.
/// </summary>
public sealed class Role : AggregateRoot, IMultiTenantEntity
{
    public Guid Id { get; private set; }

    /// <summary>
    /// Tenant this role belongs to. Stored as a plain column — no FK constraint (modular monolith boundary).
    /// </summary>
    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = default!;
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<Permission> _permissions = [];
    public IReadOnlyList<Permission> Permissions => _permissions.AsReadOnly();

    void IMultiTenantEntity.SetTenantId(Guid tenantId) => TenantId = tenantId;

    private Role() { }

    private Role(Guid id, Guid tenantId, string name, bool isSystem, List<Permission> permissions)
    {
        Id          = id;
        TenantId    = tenantId;
        Name        = name;
        IsSystem    = isSystem;
        _permissions = permissions;
    }

    public static Role Create(
        Guid tenantId,
        string name,
        IEnumerable<string> permissionCodes,
        IReadOnlySet<string> validCodes,
        bool isSystem = false,
        Guid? id = null)
    {
        if (name.Length is < 3 or > 10)
            throw new InvalidRoleNameException();

        var codes = permissionCodes.ToList();

        var unknown = codes.Except(validCodes).ToList();
        if (unknown.Count != 0)
            throw new RoleWithInvalidPermissionException(unknown);

        var role = new Role(id ?? Guid.NewGuid(), tenantId, name, isSystem, codes.Select(Permission.Of).ToList());

        role.AddDomainEvent(new RoleCreatedDomainEvent(tenantId, role.Id));

        return role;
    }

    /// <summary>
    /// Marks the role for physical deletion.
    /// The caller must remove the entity via repository after calling this.
    /// Emits RoleDeletedDomainEvent.
    /// </summary>
    public void Delete()
    {
        AddDomainEvent(new RoleDeletedDomainEvent(TenantId, Id));
    }

    /// <summary>
    /// Soft-deactivates the role when it has historical references.
    /// Emits RoleDeactivatedDomainEvent.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new RoleDeactivatedDomainEvent(TenantId, Id));
    }

    public void Rename(string name)
    {
        if (name.Length is < 3 or > 10)
            throw new InvalidRoleNameException();

        Name = name;
    }

    public void UpdatePermissions(IEnumerable<string> permissionCodes, IReadOnlySet<string> validCodes)
    {
        if (IsSystem)
            throw new SystemRoleCannotBeModifiedException(Name);

        var codes = permissionCodes.ToList();
        var unknown = codes.Except(validCodes).ToList();
        if (unknown.Count != 0)
            throw new RoleWithInvalidPermissionException(unknown);

        _permissions.Clear();
        _permissions.AddRange(codes.Select(Permission.Of));

        AddDomainEvent(new RoleUpdatedDomainEvent(TenantId, Id));
    }
}
