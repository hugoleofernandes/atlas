using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.Modules;

namespace Atlas.Party.Contracts.Permissions;

public sealed partial class PartyModulePermissions
{
    public static class Organization
    {
        public static readonly PermissionDefinition Read       = new("party.organization.read",       false, AtlasModules.Party);
        public static readonly PermissionDefinition Create     = new("party.organization.create",     false, AtlasModules.Party);
        public static readonly PermissionDefinition Update     = new("party.organization.update",     false, AtlasModules.Party);
        public static readonly PermissionDefinition Deactivate = new("party.organization.deactivate", false, AtlasModules.Party);
        public static readonly PermissionDefinition Manage     = new("party.organization.manage",     true,  AtlasModules.Party);
    }
}
