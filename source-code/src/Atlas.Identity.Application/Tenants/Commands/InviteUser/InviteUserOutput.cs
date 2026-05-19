namespace Atlas.Identity.Application.Tenants.Commands.InviteUser;

public sealed record InviteUserOutput(
    Guid InvitationId,
    string Email,
    Guid RoleId,
    string RoleName,
    DateTime ExpiresAt
);
