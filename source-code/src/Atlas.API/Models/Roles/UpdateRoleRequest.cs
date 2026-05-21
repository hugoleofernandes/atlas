namespace Atlas.API.Models.Roles;

public sealed record UpdateRoleRequest(string Name, IReadOnlyList<string> PermissionCodes);
