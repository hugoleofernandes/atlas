using Atlas.API.Models;
using Atlas.Identity.Application.Tenants.Commands.InviteUser;

namespace Atlas.API.Models.Invitations;

public sealed record InviteUserResponse(
    Guid InvitationId,
    string Email,
    Guid RoleId,
    string RoleName,
    DateTime ExpiresAt
) : IResponseFrom<InviteUserOutput, InviteUserResponse>
{
    public static InviteUserResponse From(InviteUserOutput output)
        => new(
            output.InvitationId,
            output.Email,
            output.RoleId,
            output.RoleName,
            output.ExpiresAt);
}
