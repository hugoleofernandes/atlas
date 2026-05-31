namespace Atlas.SharedDomain.Identity;

/// <summary>
/// Deterministic GUIDs for Identity module aggregate roots.
/// Format: 0000000{module}-0000-0000-0000-0000000000{entity}
/// Used in the audit trail (entity_type_id column) and in the Platform EntityTypes registry.
/// </summary>
public static class IdentityEntityTypes
{
    public static readonly Guid User       = new("00000001-0000-0000-0000-000000000001");
    public static readonly Guid Role       = new("00000001-0000-0000-0000-000000000002");
    public static readonly Guid Invitation = new("00000001-0000-0000-0000-000000000003");
}
