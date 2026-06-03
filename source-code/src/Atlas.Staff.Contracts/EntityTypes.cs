namespace Atlas.Staff.Contracts;

/// <summary>
/// Deterministic GUIDs for Staff module aggregate roots.
/// Format: 0000000{module}-0000-0000-0000-0000000000{entity}
/// </summary>
public static class EntityTypes
{
    public static readonly Guid StaffMemberId = new("00000002-0000-0000-0000-000000000001");
}
