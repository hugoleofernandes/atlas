using Atlas.SharedKernel.Modules;

namespace Atlas.SharedKernel.EntityTypes;

/// <summary>
/// Deterministic GUIDs for Staff module aggregate roots.
/// Format: 0000000{module}-0000-0000-0000-0000000000{entity}
/// </summary>
public static class StaffEntityTypes
{
    public static AtlasModule Module => AtlasModules.Staff;

    public static readonly AtlasEntityType StaffMember = AtlasEntityType.Create("1", "StaffMember", Module);
}
