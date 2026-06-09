using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.Identity.Domain.Tenants._Roles.Exceptions;
using Atlas.Identity.Domain.Tenants.Events;
using Atlas.SharedKernel.Domain;
using Atlas.Identity.Contracts.EntityTypes;

namespace Atlas.Identity.Domain.Tenants._Roles;

/// <summary>
/// A named set of permissions scoped to a tenant.
/// Permissions are referenced by PermissionId — metadata lives in the Identity permission catalog.
/// </summary>
public sealed class Role : AggregateRoot, IMultiTenantEntity, IAuditableAggregate
{
    public Guid EntityTypeId => IdentityModuleEntityTypes.Roles.EntityType.Id;

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = default!;
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<RolePermission> _permissions = [];
    public IReadOnlyList<RolePermission> Permissions => _permissions.AsReadOnly();

    void IMultiTenantEntity.SetTenantId(Guid tenantId) => TenantId = tenantId;

    private Role() { }

    private Role(Guid id, Guid tenantId, string name, bool isSystem, List<RolePermission> permissions)
    {
        Id = id;
        TenantId = tenantId;
        Name = name;
        IsSystem = isSystem;
        _permissions = permissions;
    }

    public static Role Create(
        Guid tenantId,
        string name,
        IEnumerable<RolePermission> permissions,
        bool isSystem = false,
        Guid? id = null
    )
    {
        if (name.Length is < 3 or > 10)
            throw new InvalidRoleNameException();

        var normalizedPermissions = permissions
            .DistinctBy(p => p.PermissionId)
            .ToList();

        var role = new Role(id ?? Guid.NewGuid(), tenantId, name, isSystem, normalizedPermissions);

        role.AddDomainEvent(new RoleCreatedDomainEvent(tenantId, role.Id));

        return role;
    }

    public void Delete()
    {
        AddDomainEvent(new RoleDeletedDomainEvent(TenantId, Id));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        AddDomainEvent(new RoleDeactivatedDomainEvent(TenantId, Id));
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        AddDomainEvent(new RoleActivatedDomainEvent(TenantId, Id));
    }

    public void Rename(string name)
    {
        if (name.Length is < 3 or > 10)
            throw new InvalidRoleNameException();

        Name = name;
    }

    public void UpdatePermissions(IEnumerable<RolePermission> permissions)
    {
        if (IsSystem)
            throw new SystemRoleCannotBeModifiedException(Name);

        var normalizedPermissions = permissions
            .DistinctBy(p => p.PermissionId)
            .ToList();

        _permissions.Clear();
        _permissions.AddRange(normalizedPermissions);

        AddDomainEvent(new RoleUpdatedDomainEvent(TenantId, Id));
    }
}
