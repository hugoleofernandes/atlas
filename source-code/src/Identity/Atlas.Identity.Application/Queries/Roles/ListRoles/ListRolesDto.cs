namespace Atlas.Identity.Application.Queries.Roles.ListRoles;

public sealed record ListRolesDto(
    Guid RoleId,
    string Name,
    bool IsSystem,
    bool IsActive,
    DateTime CreatedAt,
    Guid? CreatedBy,
    string? CreatedByEmail,
    DateTime? UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByEmail,
    IReadOnlyList<string> PermissionCodes
);
