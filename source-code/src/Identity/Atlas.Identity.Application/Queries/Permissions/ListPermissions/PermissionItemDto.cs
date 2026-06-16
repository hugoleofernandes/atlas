namespace Atlas.Identity.Application.Queries.Permissions.ListPermissions;

public sealed record PermissionItemDto(Guid ModuleId, string ModuleName, string Code, string Group);
