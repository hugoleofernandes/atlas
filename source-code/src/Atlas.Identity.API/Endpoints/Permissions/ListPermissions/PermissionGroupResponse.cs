namespace Atlas.Identity.API.Endpoints.Permissions.ListPermissions;

public sealed record PermissionItemResponse(string Code, string Label);

public sealed record PermissionGroupResponse(
    PermissionItemResponse          Manage,
    IReadOnlyList<PermissionItemResponse> Granular
);
