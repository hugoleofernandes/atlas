namespace Atlas.Identity.Application.Aggregates.Tenants._Roles._Permissions.Handlers.Queries.ListPermissions;

public sealed record PermissionGroupDto(string Manage, IReadOnlyList<string> Granular);
