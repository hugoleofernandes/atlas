using Atlas.SharedKernel.Modules;

namespace Atlas.SharedKernel.EntityTypes;

/// <summary>
/// Deterministic GUIDs for Identity module aggregate roots.
/// Format: 0000000{module}-0000-0000-0000-0000000000{entity}
/// Used in the audit trail (entity_type_id column) and in the Platform EntityTypes registry.
/// </summary>
public static class IdentityEntityTypes
{
    public static AtlasModule Module => AtlasModules.Identity;

    public static readonly AtlasEntityType User = AtlasEntityType.Create("1", "User", Module);
    public static readonly AtlasEntityType Role = AtlasEntityType.Create("2", "Role", Module);
    public static readonly AtlasEntityType Invitation = AtlasEntityType.Create("3", "Invitation", Module);
}
