using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.Modules;

namespace Atlas.Identity.Contracts.Permissions;

public sealed partial class IdentityModulePermissions
{
    public static class Invitations
    {
        public static readonly PermissionDefinition Read = new(
            "identity.invitations.read",
            false,
            AtlasModules.Identity
        );
        public static readonly PermissionDefinition Create = new(
            "identity.invitations.create",
            false,
            AtlasModules.Identity
        );
        public static readonly PermissionDefinition Update = new(
            "identity.invitations.update",
            false,
            AtlasModules.Identity
        );
        public static readonly PermissionDefinition Delete = new(
            "identity.invitations.delete",
            false,
            AtlasModules.Identity
        );
        public static readonly PermissionDefinition Manage = new(
            "identity.invitations.manage",
            true,
            AtlasModules.Identity
        );
    }
}
