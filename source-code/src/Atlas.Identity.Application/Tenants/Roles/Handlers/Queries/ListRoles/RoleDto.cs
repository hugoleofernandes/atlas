namespace Atlas.Identity.Application.Tenants.Roles.Handlers.Queries.ListRoles;

public sealed record RoleDto(
    Guid RoleId,
    string Name,
    bool IsSystem,
    IReadOnlyList<string> PermissionCodes
);
