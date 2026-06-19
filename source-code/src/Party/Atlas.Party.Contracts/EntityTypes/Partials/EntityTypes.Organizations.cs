using Atlas.SharedKernel.Modules;

namespace Atlas.Party.Contracts.EntityTypes;

public sealed partial class PartyModuleEntityTypes
{
    public static class Organizations
    {
        public static readonly AtlasEntityType EntityType =
            AtlasEntityType.Create("2", "Organization", AtlasModules.Party);
    }
}
