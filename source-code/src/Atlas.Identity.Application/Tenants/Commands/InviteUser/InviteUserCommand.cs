namespace Atlas.Identity.Application.Tenants.Commands.InviteUser;

public sealed record InviteUserCommand(
    string Email,
    Guid RoleId
);
