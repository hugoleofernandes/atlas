using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.Permissions;
using Atlas.Identity.Domain.ValueObjects;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants;

/// <summary>
/// A named set of permissions scoped to a tenant.
///
/// Invariants:
/// - System roles (IsSystem=true) cannot have their permissions modified or be deleted.
/// - Every permission code must exist in PermissionCatalog.All (or AllIncludingSystem for system roles).
/// - Role name is unique within the tenant (enforced by Tenant aggregate).
///
/// Design: Role is an entity owned by the Tenant aggregate.
/// Clients configure which permissions each role has from the static catalog.
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

    internal static Role Create(
        Guid tenantId,
        string name,
        IEnumerable<string> permissionCodes,
        bool isSystem = false,
        Guid? id = null)
    {
        if (name.Length is < 3 or > 10)
            throw new InvalidRoleNameException();

        var codes = permissionCodes.ToList();

        var validSet = isSystem ? PermissionCatalog.AllIncludingSystem : PermissionCatalog.All;
        var unknown = codes.Except(validSet).ToList();
        if (unknown.Count != 0)
            throw new InvalidPermissionException(unknown);

        return new Role(id ?? Guid.NewGuid(), tenantId, name, isSystem, codes.Select(Permission.Of).ToList());
    }

    internal void UpdatePermissions(IEnumerable<string> permissionCodes)
    {
        if (IsSystem)
            throw new SystemRoleCannotBeModifiedException(Name);

        var codes = permissionCodes.ToList();
        var unknown = codes.Except(PermissionCatalog.All).ToList();
        if (unknown.Count != 0)
            throw new InvalidPermissionException(unknown);

        _permissions.Clear();
        _permissions.AddRange(codes.Select(Permission.Of));
    }
}
