using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.Modules;

namespace Atlas.Identity.Contracts.Permissions;

public sealed partial class IdentityModulePermissions
{
    public static class Audit
    {
        public static readonly PermissionDefinition Read = new("identity.audit.read", false, AtlasModules.Identity);
        public static readonly PermissionDefinition Manage = new("identity.audit.manage", true, AtlasModules.Identity);
    }
}
