namespace Atlas.Identity.Application.Commands.UpdateRole;

public sealed record UpdateRoleCommand(Guid RoleId, string Name, IReadOnlyList<string> PermissionCodes);
