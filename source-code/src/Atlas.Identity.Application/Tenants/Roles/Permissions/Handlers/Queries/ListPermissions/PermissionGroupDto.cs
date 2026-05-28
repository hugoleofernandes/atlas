namespace Atlas.Identity.Application.Tenants.Roles.Permissions.Handlers.Queries.ListPermissions;

public sealed record PermissionGroupDto(string Manage, IReadOnlyList<string> Granular);
