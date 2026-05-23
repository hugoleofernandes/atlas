namespace Atlas.Identity.Application.Tenants.Queries.Dtos;

public sealed record InvitationDto(
    Guid InvitationId,
    string Email,
    Guid RoleId,
    string RoleName,
    DateTime ExpiresAt,
    bool IsUsed
);
