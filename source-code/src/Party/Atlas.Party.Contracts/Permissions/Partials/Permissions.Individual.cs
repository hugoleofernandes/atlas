using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.Modules;

namespace Atlas.Party.Contracts.Permissions;

public sealed partial class PartyModulePermissions
{
    public static class Individual
    {
        public static readonly PermissionDefinition Read       = new("party.individual.read",       false, AtlasModules.Party);
        public static readonly PermissionDefinition Create     = new("party.individual.create",     false, AtlasModules.Party);
        public static readonly PermissionDefinition Update     = new("party.individual.update",     false, AtlasModules.Party);
        public static readonly PermissionDefinition Deactivate = new("party.individual.deactivate", false, AtlasModules.Party);
        public static readonly PermissionDefinition Manage     = new("party.individual.manage",     true,  AtlasModules.Party);
    }
}
