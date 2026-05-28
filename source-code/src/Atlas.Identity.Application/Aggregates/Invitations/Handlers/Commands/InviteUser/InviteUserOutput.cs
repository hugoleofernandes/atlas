namespace Atlas.Identity.Application.Aggregates.Invitations.Handlers.Commands.InviteUser;

public sealed record InviteUserOutput(
    Guid InvitationId,
    string Email,
    Guid RoleId,
    string RoleName,
    DateTime ExpiresAt
);
