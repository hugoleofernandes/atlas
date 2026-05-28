using Atlas.SharedKernel.Domain.Permissions;

namespace Atlas.Identity.Domain.Tenants._Roles._Permissions;

/// <summary>
/// Single source of truth for Identity module permissions.
/// Declares the permission codes as typed constants (used in [HasPermission] attributes)
/// and implements IModulePermissions so the codes and groups are automatically derived
/// via PermissionExtractor — no duplication required.
///
/// Convention: a nested class with a "Manage" field produces one PermissionGroup.
/// </summary>
public sealed class IdentityModulePermissions : IModulePermissions
{
    public static class Tenant
    {
        public static class Roles
        {
            public const string Read   = "tenant.roles.read";
            public const string Create = "tenant.roles.create";
            public const string Update = "tenant.roles.update";
            public const string Delete = "tenant.roles.delete";
            public const string Manage = "tenant.roles.manage";
        }

        public static class Invitations
        {
            public const string Read   = "tenant.invitations.read";
            public const string Create = "tenant.invitations.create";
            public const string Update = "tenant.invitations.update";
            public const string Delete = "tenant.invitations.delete";
            public const string Manage = "tenant.invitations.manage";
        }
    }

    public IReadOnlySet<string> Permissions { get; }
        = PermissionExtractor.ExtractAll(typeof(IdentityModulePermissions));

    public IReadOnlyList<PermissionGroup> Groups { get; }
        = PermissionExtractor.ExtractGroups(typeof(IdentityModulePermissions));
}
