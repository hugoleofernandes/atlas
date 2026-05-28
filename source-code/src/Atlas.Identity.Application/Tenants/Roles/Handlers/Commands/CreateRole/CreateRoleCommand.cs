namespace Atlas.Identity.Application.Tenants.Roles.Handlers.Commands.CreateRole;

public sealed record CreateRoleCommand(
    string Name,
    IEnumerable<string> PermissionCodes
);
