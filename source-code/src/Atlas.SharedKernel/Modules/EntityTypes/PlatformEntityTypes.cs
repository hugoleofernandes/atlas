using Atlas.SharedKernel.Modules;

namespace Atlas.SharedKernel.EntityTypes;

/// <summary>
/// Deterministic GUIDs for Platform module aggregate roots.
/// Format: 0000000{module}-0000-0000-0000-0000000000{entity}
/// </summary>
public static class PlatformEntityTypes
{
    public static AtlasModule Module => AtlasModules.Platform;

    public static readonly AtlasEntityType Tenant = AtlasEntityType.Create("1", "Tenant", Module);
}
