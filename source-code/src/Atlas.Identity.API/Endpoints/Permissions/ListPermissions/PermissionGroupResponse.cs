namespace Atlas.Identity.API.Endpoints.Permissions.ListPermissions;

public sealed record PermissionItemResponse(string Code, string Label);

public sealed record PermissionGroupResponse(
    PermissionItemResponse          Manage,
    IReadOnlyList<PermissionItemResponse> Granular
);

public sealed record PermissionModuleResponse(
    Guid ModuleId,
    string ModuleName,
    IReadOnlyList<PermissionGroupResponse> Groups
);
