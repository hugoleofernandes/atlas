namespace Atlas.Identity.API.Endpoints.Roles.UpdateRole;

public sealed record UpdateRoleRequest(Guid Id, string Name, IReadOnlyList<string> PermissionCodes);
