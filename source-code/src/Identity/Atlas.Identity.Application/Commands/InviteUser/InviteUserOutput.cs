namespace Atlas.Identity.Application.Commands.InviteUser;

public sealed record InviteUserOutput(
    Guid InvitationId,
    string Email,
    Guid RoleId,
    string RoleName,
    DateTime ExpiresAt
);
