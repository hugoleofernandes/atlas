using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.Modules;

namespace Atlas.Identity.Contracts.Permissions;

public sealed partial class IdentityModulePermissions
{
    public static class Roles
    {
        public static readonly PermissionDefinition Read = new("identity.roles.read", false, AtlasModules.Identity);
        public static readonly PermissionDefinition Create = new("identity.roles.create", false, AtlasModules.Identity);
        public static readonly PermissionDefinition Update = new("identity.roles.update", false, AtlasModules.Identity);
        public static readonly PermissionDefinition Delete = new("identity.roles.delete", false, AtlasModules.Identity);
        public static readonly PermissionDefinition Manage = new("identity.roles.manage", true, AtlasModules.Identity);
    }
}
