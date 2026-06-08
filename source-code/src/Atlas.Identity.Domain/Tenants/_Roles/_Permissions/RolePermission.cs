using Atlas.SharedKernel.Domain;

namespace Atlas.Identity.Domain.Tenants._Roles._Permissions;

/// <summary>
/// References a catalog permission assigned to a Role.
/// Stores only the PermissionId — metadata (Code, Group, IsManager) lives in the catalog.
/// </summary>
public sealed class RolePermission : ValueObject
{
    public Guid PermissionId { get; }

    private RolePermission(Guid permissionId) => PermissionId = permissionId;

    public static RolePermission Of(Guid permissionId) => new(permissionId);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return PermissionId;
    }
}
