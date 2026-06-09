namespace Atlas.Identity.Application.Queries.Invitations.ListInvitations;

public interface IListInvitationsReader
{
    Task<IReadOnlyList<InvitationDto>> ListAsync(Guid tenantId, bool? isActive, CancellationToken ct);
}
