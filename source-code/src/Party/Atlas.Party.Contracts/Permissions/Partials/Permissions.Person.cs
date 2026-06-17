using Atlas.BuildingBlocks.Permissions;
using Atlas.SharedKernel.Modules;

namespace Atlas.Party.Contracts.Permissions;

public sealed partial class PartyModulePermissions
{
    public static class Person
    {
        public static readonly PermissionDefinition Read       = new("party.person.read",       false, AtlasModules.Party);
        public static readonly PermissionDefinition Create     = new("party.person.create",     false, AtlasModules.Party);
        public static readonly PermissionDefinition Update     = new("party.person.update",     false, AtlasModules.Party);
        public static readonly PermissionDefinition Deactivate = new("party.person.deactivate", false, AtlasModules.Party);
        public static readonly PermissionDefinition Manage     = new("party.person.manage",     true,  AtlasModules.Party);
    }
}

