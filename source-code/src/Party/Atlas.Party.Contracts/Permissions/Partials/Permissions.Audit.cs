using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.Modules;

namespace Atlas.Party.Contracts.Permissions;

public sealed partial class PartyModulePermissions
{
    public static class Audit
    {
        public static readonly PermissionDefinition Read = new("party.audit.read", false, AtlasModules.Party);
        public static readonly PermissionDefinition Manage = new("party.audit.manage", true, AtlasModules.Party);
    }
}
