namespace Atlas.BuildingBlocks.Permissions;

/// <summary>
/// Permission catalog owned by a module.
/// Keeps the API contract explicit instead of inferring ownership from permission prefixes.
/// </summary>
public sealed record ModulePermissionCatalog(
    Guid ModuleId,
    string ModuleName,
    IReadOnlySet<string> Permissions,
    IReadOnlyList<PermissionGroup> Groups);
