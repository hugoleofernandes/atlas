namespace Atlas.API.Models.Roles;

public sealed record UpdateRoleResponse(Guid RoleId, string Name, IReadOnlyList<string> PermissionCodes);
