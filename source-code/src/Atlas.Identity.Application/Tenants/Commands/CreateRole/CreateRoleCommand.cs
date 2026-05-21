namespace Atlas.Identity.Application.Tenants.Commands.CreateRole;

public sealed record CreateRoleCommand(
    string Name,
    IEnumerable<string> PermissionCodes
);
