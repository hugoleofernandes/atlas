namespace Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Commands.UpdateRole;

public sealed record UpdateRoleCommand(Guid RoleId, string Name, IReadOnlyList<string> PermissionCodes);
