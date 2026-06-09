namespace Atlas.Identity.Application.Commands.CreateRole;

public sealed record CreateRoleOutput(
    Guid RoleId,
    string Name,
    bool IsActive,
    IReadOnlyList<string> PermissionCodes
);
