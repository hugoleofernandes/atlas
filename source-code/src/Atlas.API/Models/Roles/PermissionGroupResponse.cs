namespace Atlas.API.Models.Roles;

public sealed record PermissionItemResponse(string Code, string Label);

public sealed record PermissionGroupResponse(
    PermissionItemResponse Manage,
    IReadOnlyList<PermissionItemResponse> Granular
);
