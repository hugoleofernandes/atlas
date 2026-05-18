namespace Atlas.API.Models.Roles;

public sealed record CreateRoleRequest(
    string Name,
    IEnumerable<string> PermissionCodes
);
