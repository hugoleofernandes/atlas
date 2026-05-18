using Atlas.Identity.Domain.Entities.Tenants.Exceptions;
using Atlas.Identity.Domain.ValueObjects;
using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Entities.Tenants;

/// <summary>
/// A named set of permissions scoped to a tenant.
///
/// Invariants:
/// - System roles (IsSystem=true) cannot have their permissions modified or be deleted.
/// - Every permission code must exist in PermissionCatalog.All.
/// - Role name is unique within the tenant (enforced by Tenant aggregate).
///
/// Design: TenantRole is an entity owned by the Tenant aggregate.
/// Clients configure which permissions each role has from the static catalog.
/// </summary>
public sealed class TenantRole : AuditableEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = default!;

    public bool IsSystem { get; private set; }

    private readonly List<RolePermission> _permissions = [];
    public IReadOnlyList<RolePermission> Permissions => _permissions.AsReadOnly();

    private TenantRole() { }

    private TenantRole(Guid tenantId, string name, bool isSystem, List<RolePermission> permissions)
    {
        TenantId = tenantId;
        Name = name;
        IsSystem = isSystem;
        _permissions = permissions;
    }

    internal static TenantRole Create(
        Guid tenantId,
        string name,
        IEnumerable<string> permissionCodes,
        bool isSystem = false)
    {
        var codes = permissionCodes.ToList();
        var unknown = codes.Except(PermissionCatalog.All).ToList();
        if (unknown.Count != 0)
            throw new InvalidPermissionException(unknown);

        return new TenantRole(tenantId, name, isSystem, codes.Select(RolePermission.Of).ToList());
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
        _permissions.AddRange(codes.Select(RolePermission.Of));
    }
}
