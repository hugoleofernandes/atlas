namespace Atlas.Identity.Application.Tenants.Roles.Handlers.Commands.CreateRole;

public sealed record CreateRoleOutput(
    Guid RoleId,
    string Name,
    IReadOnlyList<string> PermissionCodes
);
