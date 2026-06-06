namespace Atlas.SharedKernel.EntityTypes;

/// <summary>
/// Deterministic GUIDs for Staff module aggregate roots.
/// Format: 0000000{module}-0000-0000-0000-0000000000{entity}
/// </summary>
public static class StaffEntityTypes
{
    public static readonly Guid ModuleId = new("e24bf9cf-84aa-4f55-884c-9784e2278d2e");
    public const string ModuleName = "staff";

    public static readonly Guid StaffMemberId = new("00000002-0000-0000-0000-000000000001");
}
