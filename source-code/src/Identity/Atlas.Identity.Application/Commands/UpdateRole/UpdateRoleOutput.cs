namespace Atlas.Identity.Application.Commands.UpdateRole;

public sealed record UpdateRoleOutput(Guid RoleId, string Name, bool IsActive, IReadOnlyList<string> PermissionCodes);
