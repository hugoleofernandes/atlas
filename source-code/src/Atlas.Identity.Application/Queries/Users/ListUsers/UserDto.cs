namespace Atlas.Identity.Application.Queries.Users.ListUsers;

public sealed record UserDto(
    Guid     UserId,
    string   Email,
    Guid     RoleId,
    string   RoleName,
    bool     IsActive,
    DateTime CreatedAt,
    Guid?    CreatedBy,
    string?  CreatedByEmail,
    DateTime? UpdatedAt,
    Guid?    UpdatedBy,
    string?  UpdatedByEmail);
