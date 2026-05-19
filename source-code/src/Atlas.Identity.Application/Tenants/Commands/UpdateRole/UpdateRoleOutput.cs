namespace Atlas.Identity.Application.Tenants.Commands.UpdateRole;

public sealed record UpdateRoleOutput(Guid RoleId, string Name, IReadOnlyList<string> PermissionCodes);
