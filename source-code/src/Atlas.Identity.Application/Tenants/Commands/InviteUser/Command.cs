namespace Atlas.Identity.Application.Tenants.Commands.InviteUser;

public sealed record Command(
    string Email,
    Guid RoleId
);
