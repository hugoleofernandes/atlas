namespace Atlas.Identity.Application.Queries.Permissions.ListPermissions;

public sealed record PermissionGroupDto(string Manage, IReadOnlyList<string> Granular);
