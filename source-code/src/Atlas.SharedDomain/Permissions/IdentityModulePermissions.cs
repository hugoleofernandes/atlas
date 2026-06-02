using Atlas.SharedDomain.Modules;
using Atlas.SharedKernel.Domain.Permissions;

namespace Atlas.SharedDomain.Permissions;

/// <summary>
/// Canonical permission codes for Identity-owned product capabilities.
/// Permission codes are shared contracts used by authorization, role management,
/// claims, and frontend permission catalogs.
/// </summary>
public sealed class IdentityModulePermissions : IModulePermissions
{
    public Guid ModuleId => AtlasModules.Identity;
    public string ModuleName => AtlasModules.IdentityName;

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

        public static class Audit
        {
            public const string Read   = "tenant.audit.read";
            public const string Manage = "tenant.audit.manage";
        }
    }

    public IReadOnlySet<string> Permissions { get; }
        = PermissionExtractor.ExtractAll(typeof(IdentityModulePermissions));

    public IReadOnlyList<PermissionGroup> Groups { get; }
        = PermissionExtractor.ExtractGroups(typeof(IdentityModulePermissions));
}
