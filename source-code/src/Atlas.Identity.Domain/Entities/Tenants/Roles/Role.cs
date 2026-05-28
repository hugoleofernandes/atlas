using Atlas.Identity.Domain.Entities.Tenants.Roles.Exceptions;
using Atlas.Identity.Domain.Entities.Tenants.Roles.Permissions;
using Atlas.SharedKernel.Domain;
using Atlas.SharedKernel.Domain.Permissions;

namespace Atlas.Identity.Domain.Entities.Tenants.Roles;

/// <summary>
/// A named set of permissions scoped to a tenant.
///
/// Invariants:
/// - System roles (IsSystem=true) cannot have their permissions modified or be deleted.
/// - Every permission code must exist in the provided validCodes set (supplied by the caller from IPermissionPolicy).
/// - Role name is unique within the tenant (enforced by Tenant aggregate).
///
/// Design: Role is an entity owned by the Tenant aggregate.
/// The domain does not reference IPermissionPolicy directly — callers pass the valid set as a parameter,
/// keeping the domain pure and enabling modular permission registration.
/// </summary>
public sealed class Role : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; } = true;
    private readonly List<Permission> _permissions = [];
    public IReadOnlyList<Permission> Permissions => _permissions.AsReadOnly();

    private Role() { }

    private Role(Guid id, Guid tenantId, string name, bool isSystem, List<Permission> permissions)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
        IsSystem = isSystem;
        _permissions = permissions;
    }

    internal void Deactivate()
    {
        IsActive = false;
    }

    internal void Rename(string name)
    {
        if (name.Length is < 3 or > 10)
            throw new InvalidRoleNameException();

        Name = name;
    }

    internal static Role Create(
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

        return new Role(id ?? Guid.NewGuid(), tenantId, name, isSystem, codes.Select(Permission.Of).ToList());
    }

    internal void UpdatePermissions(IEnumerable<string> permissionCodes, IReadOnlySet<string> validCodes)
    {
        if (IsSystem)
            throw new SystemRoleCannotBeModifiedException(Name);

        var codes = permissionCodes.ToList();
        var unknown = codes.Except(validCodes).ToList();
        if (unknown.Count != 0)
            throw new RoleWithInvalidPermissionException(unknown);

        _permissions.Clear();
        _permissions.AddRange(codes.Select(Permission.Of));
    }
}
