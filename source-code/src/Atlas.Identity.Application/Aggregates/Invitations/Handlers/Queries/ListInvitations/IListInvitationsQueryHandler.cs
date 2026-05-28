using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Aggregates.Invitations.Handlers.Queries.ListInvitations;

public interface IListInvitationsQueryHandler : IQueryHandler<ListInvitationsQuery, IReadOnlyList<InvitationDto>>
{
}
