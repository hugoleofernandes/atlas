using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.Modules;

namespace Atlas.Identity.Contracts.Permissions;

public sealed partial class IdentityModulePermissions
{
    public static class Users
    {
        public static readonly PermissionDefinition Read = new("identity.users.read", false, AtlasModules.Identity);
    }
}
