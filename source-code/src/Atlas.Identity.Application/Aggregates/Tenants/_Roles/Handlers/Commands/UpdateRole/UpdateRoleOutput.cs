namespace Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Commands.UpdateRole;

public sealed record UpdateRoleOutput(Guid RoleId, string Name, IReadOnlyList<string> PermissionCodes);
