namespace Atlas.API.Models.Invitations;

public sealed record InviteUserRequest(
    string Email,
    Guid RoleId
);
