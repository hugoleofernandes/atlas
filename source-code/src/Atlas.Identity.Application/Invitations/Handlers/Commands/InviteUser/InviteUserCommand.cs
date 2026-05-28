namespace Atlas.Identity.Application.Invitations.Handlers.Commands.InviteUser;

public sealed record InviteUserCommand(
    string Email,
    Guid RoleId
);
