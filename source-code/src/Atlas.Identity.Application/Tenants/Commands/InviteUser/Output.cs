namespace Atlas.Identity.Application.Tenants.Commands.InviteUser;

public sealed record Output(
    Guid InvitationId,
    string Email,
    Guid RoleId,
    string RoleName,
    DateTime ExpiresAt
);
