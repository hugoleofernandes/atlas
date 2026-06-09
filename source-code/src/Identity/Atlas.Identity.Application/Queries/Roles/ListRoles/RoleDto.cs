namespace Atlas.Identity.Application.Queries.Roles.ListRoles;

public sealed record RoleDto(
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
