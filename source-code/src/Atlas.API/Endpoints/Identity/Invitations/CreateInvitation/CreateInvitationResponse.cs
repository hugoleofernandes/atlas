using Atlas.Identity.Application.Invitations.Handlers.Commands.InviteUser;

namespace Atlas.API.Endpoints.Identity.Invitations.CreateInvitation;

public sealed record CreateInvitationResponse(
    Guid InvitationId,
    string Email,
    Guid RoleId,
    string RoleName,
    DateTime ExpiresAt)
{
    public static CreateInvitationResponse From(InviteUserOutput output)
        => new(output.InvitationId, output.Email, output.RoleId, output.RoleName, output.ExpiresAt);
}
