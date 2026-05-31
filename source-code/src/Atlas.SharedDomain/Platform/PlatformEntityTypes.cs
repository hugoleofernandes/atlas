namespace Atlas.SharedDomain.Platform;

/// <summary>
/// Deterministic GUIDs for Platform module aggregate roots.
/// Format: 0000000{module}-0000-0000-0000-0000000000{entity}
/// Used in the audit trail (entity_type_id column) and in the Platform EntityTypes registry.
/// </summary>
public static class PlatformEntityTypes
{
    public static readonly Guid Tenant = new("00000003-0000-0000-0000-000000000001");
}
