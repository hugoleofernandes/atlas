namespace Atlas.SharedKernel.EntityTypes;

/// <summary>
/// Deterministic GUIDs for Platform module aggregate roots.
/// Format: 0000000{module}-0000-0000-0000-0000000000{entity}
/// </summary>
public static class PlatformEntityTypes
{
    public static readonly Guid ModuleId = new("b4d35314-b7b1-4a56-8bb6-fd1beedf5070");
    public const string ModuleName = "platform";

    public static readonly Guid RootTenantId = new("00000003-0000-0000-0000-000000000001");
}
