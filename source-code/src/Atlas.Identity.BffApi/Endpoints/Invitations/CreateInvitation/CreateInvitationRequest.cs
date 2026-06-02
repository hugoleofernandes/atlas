namespace Atlas.Identity.BffApi.Endpoints.Invitations.CreateInvitation;

public sealed record CreateInvitationRequest(string Email, Guid RoleId);
