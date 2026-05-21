namespace Atlas.API.Models.Invitations;

public sealed record InviteUserResponse(
    Guid InvitationId,
    string Email,
    Guid RoleId,
    string RoleName,
    DateTime ExpiresAt
);