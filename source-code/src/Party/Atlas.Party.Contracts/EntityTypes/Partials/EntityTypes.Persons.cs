using Atlas.SharedKernel.Modules;

namespace Atlas.Party.Contracts.EntityTypes;

public sealed partial class PartyModuleEntityTypes
{
    public static class Persons
    {
        public static readonly AtlasEntityType EntityType =
            AtlasEntityType.Create("1", "Person", AtlasModules.Party);
    }
}
