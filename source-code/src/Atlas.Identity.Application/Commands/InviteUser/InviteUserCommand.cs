namespace Atlas.Identity.Application.Commands.InviteUser;

public sealed record InviteUserCommand(
    string Email,
    Guid RoleId
);
