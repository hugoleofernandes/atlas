using Atlas.SharedKernel.Modules;

namespace Atlas.Identity.Contracts.EntityTypes;

public sealed partial class IdentityModuleEntityTypes
{
    public static class Invitations
    {
        public static readonly AtlasEntityType EntityType =
            AtlasEntityType.Create("3", "Invitation", AtlasModules.Identity);
    }
}
