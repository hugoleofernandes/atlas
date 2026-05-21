namespace Atlas.Identity.Application.Tenants.Commands.CreateRole;

public sealed record CreateRoleOutput(
    Guid RoleId,
    string Name,
    IReadOnlyList<string> PermissionCodes
);
