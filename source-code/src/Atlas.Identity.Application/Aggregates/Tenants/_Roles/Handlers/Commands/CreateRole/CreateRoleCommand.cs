namespace Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Commands.CreateRole;

public sealed record CreateRoleCommand(
    string Name,
    IEnumerable<string> PermissionCodes
);
