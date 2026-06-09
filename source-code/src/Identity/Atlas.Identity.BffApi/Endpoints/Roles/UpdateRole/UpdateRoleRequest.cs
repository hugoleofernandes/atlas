namespace Atlas.Identity.BffApi.Endpoints.Roles.UpdateRole;

public sealed record UpdateRoleRequest(Guid Id, string Name, IReadOnlyList<string> PermissionCodes);
