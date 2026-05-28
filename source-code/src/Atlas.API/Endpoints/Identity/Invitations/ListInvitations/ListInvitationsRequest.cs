namespace Atlas.API.Endpoints.Identity.Invitations.ListInvitations;

public sealed class ListInvitationsRequest
{
    public bool IsActive { get; init; } = true;
}
