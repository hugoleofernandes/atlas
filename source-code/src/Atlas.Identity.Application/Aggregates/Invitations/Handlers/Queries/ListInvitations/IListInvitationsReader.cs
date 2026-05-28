namespace Atlas.Identity.Application.Aggregates.Invitations.Handlers.Queries.ListInvitations;

public interface IListInvitationsReader
{
    Task<IReadOnlyList<InvitationDto>> ListAsync(Guid tenantId, bool isActive, CancellationToken ct);
}
