namespace Atlas.Identity.Application.Tenants.Roles.Handlers.Commands.UpdateRole;

public sealed record UpdateRoleOutput(Guid RoleId, string Name, IReadOnlyList<string> PermissionCodes);
