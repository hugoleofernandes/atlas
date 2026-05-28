namespace Atlas.API.Endpoints.Identity.Invitations.CreateInvitation;

public sealed record CreateInvitationRequest(string Email, Guid RoleId);
