using Atlas.SharedKernel.Modules;

namespace Atlas.Platform.Contracts.EntityTypes;

public sealed partial class PlatformModuleEntityTypes
{
    public static class Tenants
    {
        public static readonly AtlasEntityType EntityType =
            AtlasEntityType.Create("1", "Tenant", AtlasModules.Platform);
    }
}
