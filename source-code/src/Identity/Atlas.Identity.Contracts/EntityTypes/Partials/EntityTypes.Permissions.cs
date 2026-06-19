using Atlas.SharedKernel.Modules;

namespace Atlas.Identity.Contracts.EntityTypes;

public sealed partial class IdentityModuleEntityTypes
{
    public static class Permissions
    {
        public static readonly AtlasEntityType EntityType =
            AtlasEntityType.Create("4", "Permission", AtlasModules.Identity);
    }
}
