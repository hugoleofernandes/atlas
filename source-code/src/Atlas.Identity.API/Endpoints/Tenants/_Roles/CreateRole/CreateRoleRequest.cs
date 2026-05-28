namespace Atlas.Identity.API.Endpoints.Tenants._Roles.CreateRole;

public sealed record CreateRoleRequest(string Name, IEnumerable<string> PermissionCodes);
