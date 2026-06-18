using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.Modules;

namespace Atlas.Party.Contracts.Permissions;

public sealed partial class PartyModulePermissions
{
    public static class Lookups
    {
        public static readonly PermissionDefinition Read = new("party.lookups.read", false, AtlasModules.Party);
    }
}
