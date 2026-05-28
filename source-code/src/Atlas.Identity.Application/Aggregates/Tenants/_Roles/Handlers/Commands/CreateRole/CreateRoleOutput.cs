namespace Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Commands.CreateRole;

public sealed record CreateRoleOutput(
    Guid RoleId,
    string Name,
    IReadOnlyList<string> PermissionCodes
);
