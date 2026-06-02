namespace Atlas.Identity.Application.Queries.Permissions.ListPermissions;

public sealed record PermissionModuleDto(
    Guid ModuleId,
    string ModuleName,
    IReadOnlyList<PermissionGroupDto> Groups);
