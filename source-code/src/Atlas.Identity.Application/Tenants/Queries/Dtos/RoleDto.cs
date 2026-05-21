namespace Atlas.Identity.Application.Tenants.Queries.Dtos;

public sealed record RoleDto(
    Guid RoleId,
    string Name,
    bool IsSystem,
    IReadOnlyList<string> PermissionCodes
);
