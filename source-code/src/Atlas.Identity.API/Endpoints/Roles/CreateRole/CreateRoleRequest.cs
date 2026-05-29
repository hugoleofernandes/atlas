namespace Atlas.Identity.API.Endpoints.Roles.CreateRole;

public sealed record CreateRoleRequest(string Name, IEnumerable<string> PermissionCodes);
