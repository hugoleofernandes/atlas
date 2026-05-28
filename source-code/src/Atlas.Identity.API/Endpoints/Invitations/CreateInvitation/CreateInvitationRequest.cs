namespace Atlas.Identity.API.Endpoints.Invitations.CreateInvitation;

public sealed record CreateInvitationRequest(string Email, Guid RoleId);
