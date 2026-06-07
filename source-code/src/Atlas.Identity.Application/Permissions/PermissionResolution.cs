using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Domain.Tenants._Roles._Permissions;
using Atlas.Identity.Domain.Tenants._Roles.Exceptions;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Permissions;

public static class PermissionResolution
{
    public static IReadOnlyList<Permission> Resolve(
        IEnumerable<string> codes,
        IPermissionPolicy policy,
        bool allowSystemRoot = false)
    {
        var resolved = new List<Permission>();
        var unknown = new List<string>();

        foreach (var code in codes.Distinct(StringComparer.Ordinal))
        {
            if (allowSystemRoot && string.Equals(code, SystemPermissions.Root, StringComparison.Ordinal))
            {
                resolved.Add(Permission.Of(SystemPermissions.Root, "system", false));
                continue;
            }

            if (!policy.DefinitionsByCode.TryGetValue(code, out var definition))
            {
                unknown.Add(code);
                continue;
            }

            resolved.Add(Permission.Of(definition.Code, definition.Group, definition.IsManager));
        }

        if (unknown.Count > 0)
            throw new RoleWithInvalidPermissionException(unknown);

        return resolved;
    }
}
