namespace Atlas.Identity.Application.Queries.Roles.ListRoles;

public sealed record RoleDto(
    Guid RoleId,
    string Name,
    bool IsSystem,
    IReadOnlyList<string> PermissionCodes
);
