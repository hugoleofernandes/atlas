namespace Atlas.Identity.Application.Commands.CreateRole;

public sealed record CreateRoleOutput(
    Guid RoleId,
    string Name,
    IReadOnlyList<string> PermissionCodes
);
