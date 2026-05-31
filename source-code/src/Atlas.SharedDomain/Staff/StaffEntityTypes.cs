namespace Atlas.SharedDomain.Staff;

/// <summary>
/// Deterministic GUIDs for Staff module aggregate roots.
/// Format: 0000000{module}-0000-0000-0000-0000000000{entity}
/// Used in the audit trail (entity_type_id column) and in the Platform EntityTypes registry.
/// </summary>
public static class StaffEntityTypes
{
    public static readonly Guid StaffMember = new("00000002-0000-0000-0000-000000000001");
}
