using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.Identity.Contracts.Permissions;
using Atlas.Platform.Contracts.Permissions;
using Atlas.SharedKernel.Application;
using Atlas.Staff.Contracts.Permissions;

namespace Atlas.Identity.Tests;

internal static class PermissionFixtures
{
    private static readonly IPermissionPolicy Policy = new PermissionPolicyService(
    [
        new IdentityModulePermissions(),
        new StaffModulePermissions(),
        new PlatformModulePermissions(),
    ]);

    public static IReadOnlySet<string> AllCodes => Policy.All;

    public static IReadOnlySet<string> AllIncludingSystemCodes => Policy.AllIncludingSystem;

    public static IReadOnlyList<Permission> Resolve(params string[] codes)
    {
        var resolved = new List<Permission>();

        foreach (var code in codes.Distinct(StringComparer.Ordinal))
        {
            if (string.Equals(code, SystemPermissions.Root, StringComparison.Ordinal))
            {
                resolved.Add(Permission.Of(SystemPermissions.Root, "system", false));
                continue;
            }

            var definition = Policy.DefinitionsByCode[code];
            resolved.Add(Permission.Of(definition.Code, definition.Group, definition.IsManager));
        }

        return resolved;
    }
}
