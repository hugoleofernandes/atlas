namespace Atlas.Identity.Application.Queries.Users.GetUserById;

public sealed record GetUserByIdDto(
    Guid UserId,
    string Email,
    Guid RoleId,
    string RoleName,
    bool IsActive,
    DateTime CreatedAt,
    Guid? CreatedBy,
    string? CreatedByEmail,
    DateTime? UpdatedAt,
    Guid? UpdatedBy,
    string? UpdatedByEmail
);
