namespace Atlas.Identity.API.Endpoints.Invitations.ListInvitations;

public sealed class ListInvitationsRequest
{
    public bool IsActive { get; init; } = true;
}
