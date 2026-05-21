namespace Atlas.API.Models.Roles;

public sealed record CreateRoleResponse(
    Guid RoleId,
    string Name,
    IReadOnlyList<string> PermissionCodes
);
