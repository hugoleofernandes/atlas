namespace Atlas.Identity.Domain.Permissions;

/// <summary>
/// Fixed GUIDs for system roles — stable across all environments and DB rebuilds.
/// Use these constants wherever a system role ID is needed (seeds, tests, claims).
/// </summary>
public static class SystemRoleIds
{
    public static readonly Guid Root   = new("00000000-0000-0000-0000-000000000001");
    public static readonly Guid Admin  = new("00000000-0000-0000-0000-000000000002");
    public static readonly Guid Member = new("00000000-0000-0000-0000-000000000003");
}
