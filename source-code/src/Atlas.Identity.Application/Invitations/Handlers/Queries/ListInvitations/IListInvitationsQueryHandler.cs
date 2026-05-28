using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Invitations.Handlers.Queries.ListInvitations;

public interface IListInvitationsQueryHandler : IQueryHandler<ListInvitationsQuery, IReadOnlyList<InvitationDto>>
{
}
