using Atlas.SharedKernel.Modules;

namespace Atlas.Identity.Contracts.EntityTypes;

public sealed partial class IdentityModuleEntityTypes
{
    public static class Users
    {
        public static readonly AtlasEntityType EntityType =
            AtlasEntityType.Create("1", "User", AtlasModules.Identity);
    }
}
