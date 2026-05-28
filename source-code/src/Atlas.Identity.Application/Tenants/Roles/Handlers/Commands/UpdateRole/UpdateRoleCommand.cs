namespace Atlas.Identity.Application.Tenants.Roles.Handlers.Commands.UpdateRole;

public sealed record UpdateRoleCommand(Guid RoleId, string Name, IReadOnlyList<string> PermissionCodes);
