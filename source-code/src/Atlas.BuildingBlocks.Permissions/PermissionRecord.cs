namespace Atlas.BuildingBlocks.Permissions;

/// <summary>
/// Read model for a single permission from the Identity catalog.
/// </summary>
public sealed record PermissionRecord(
    Guid Id,
    Guid? ModuleId,
    string? ModuleName,
    string Code,
    string Group,
    bool IsManager,
    bool IsRoot,
    bool IsActive);
