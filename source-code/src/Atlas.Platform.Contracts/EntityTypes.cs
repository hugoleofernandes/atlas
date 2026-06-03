namespace Atlas.Platform.Contracts;

/// <summary>
/// Deterministic GUIDs for Platform module aggregate roots.
/// Format: 0000000{module}-0000-0000-0000-0000000000{entity}
/// </summary>
public static class EntityTypes
{
    public static readonly Guid TenantId = new("00000003-0000-0000-0000-000000000001");
}
