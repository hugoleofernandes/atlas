using Atlas.SharedKernel.Modules;

namespace Atlas.Identity.Contracts.EntityTypes;

public sealed partial class IdentityModuleEntityTypes
{
    public static class Roles
    {
        public static readonly AtlasEntityType EntityType =
            AtlasEntityType.Create("2", "Role", AtlasModules.Identity);
    }
}
