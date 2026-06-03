namespace Atlas.Identity.Contracts;

/// <summary>
/// Deterministic GUIDs for Identity module aggregate roots.
/// Format: 0000000{module}-0000-0000-0000-0000000000{entity}
/// Used in the audit trail (entity_type_id column) and in the Platform EntityTypes registry.
/// </summary>
public static class EntityTypes
{
    public static readonly Guid UserId = new("00000001-0000-0000-0000-000000000001");
    public static readonly Guid RoleId = new("00000001-0000-0000-0000-000000000002");
    public static readonly Guid InvitationId = new("00000001-0000-0000-0000-000000000003");
}
