namespace Atlas.Identity.Application.Invitations.Handlers.Queries.ListInvitations;

public interface IListInvitationsReader
{
    Task<IReadOnlyList<InvitationDto>> ListAsync(Guid tenantId, bool isActive, CancellationToken ct);
}
