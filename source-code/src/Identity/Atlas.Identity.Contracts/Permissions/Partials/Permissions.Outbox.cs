using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.Modules;

namespace Atlas.Identity.Contracts.Permissions;

public sealed partial class IdentityModulePermissions
{
    public static class Outbox
    {
        public static readonly PermissionDefinition Read    = new("identity.outbox.read",    false, AtlasModules.Identity);
        public static readonly PermissionDefinition Resubmit = new("identity.outbox.resubmit", false, AtlasModules.Identity);
        public static readonly PermissionDefinition Process  = new("identity.outbox.process",  false, AtlasModules.Identity);
        public static readonly PermissionDefinition Manage   = new("identity.outbox.manage",   true,  AtlasModules.Identity);
    }
}
