namespace Atlas.Identity.Application.Aggregates.Invitations.Handlers.Commands.InviteUser;

public sealed record InviteUserCommand(
    string Email,
    Guid RoleId
);
