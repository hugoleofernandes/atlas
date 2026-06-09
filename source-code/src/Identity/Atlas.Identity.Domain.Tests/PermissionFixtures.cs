using Atlas.Identity.Domain.Tenants._Roles._Permissions;

namespace Atlas.Identity.Tests;

/// <summary>
/// Test helpers for creating RolePermission objects in domain tests.
/// Uses deterministic IDs so tests are repeatable.
/// </summary>
internal static class PermissionFixtures
{
    /// <summary>Creates a RolePermission with a random PermissionId for use in domain tests.</summary>
    public static RolePermission Any() => RolePermission.Of(Guid.NewGuid());

    /// <summary>Creates multiple RolePermissions with distinct PermissionIds.</summary>
    public static IReadOnlyList<RolePermission> Many(int count) =>
        Enumerable.Range(0, count).Select(_ => Any()).ToList();

    public static RolePermission WithId(Guid id) => RolePermission.Of(id);
}
