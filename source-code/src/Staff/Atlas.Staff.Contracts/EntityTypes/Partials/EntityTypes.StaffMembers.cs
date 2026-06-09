using Atlas.SharedKernel.Modules;

namespace Atlas.Staff.Contracts.EntityTypes;

public sealed partial class StaffModuleEntityTypes
{
    public static class StaffMembers
    {
        public static readonly AtlasEntityType EntityType =
            AtlasEntityType.Create("1", "StaffMember", AtlasModules.Staff);
    }
}
