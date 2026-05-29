namespace Atlas.Identity.Application.Commands.CreateRole;

public sealed record CreateRoleCommand(
    string Name,
    IEnumerable<string> PermissionCodes
);
