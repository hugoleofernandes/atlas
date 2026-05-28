namespace Atlas.Identity.API.Endpoints.Tenants._Roles.UpdateRole;

public sealed record UpdateRoleRequest(Guid Id, string Name, IReadOnlyList<string> PermissionCodes);
