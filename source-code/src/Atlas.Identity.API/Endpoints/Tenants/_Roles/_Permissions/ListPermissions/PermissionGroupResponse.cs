namespace Atlas.Identity.API.Endpoints.Tenants._Roles._Permissions.ListPermissions;

public sealed record PermissionItemResponse(string Code, string Label);

public sealed record PermissionGroupResponse(
    PermissionItemResponse          Manage,
    IReadOnlyList<PermissionItemResponse> Granular
);
