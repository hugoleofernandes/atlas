namespace Atlas.Identity.Application.Tenants.Queries.Dtos;

public sealed record InvitationDto(
    Guid InvitationId,
    string Email,
    Guid RoleId,
    string RoleName,
    DateTime ExpiresAt,
    bool IsUsed,
    bool IsActive,
    DateTime CreatedAt,
    Guid? CreatedBy,
    string? CreatedByEmail,
    DateTime? UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByEmail
);
