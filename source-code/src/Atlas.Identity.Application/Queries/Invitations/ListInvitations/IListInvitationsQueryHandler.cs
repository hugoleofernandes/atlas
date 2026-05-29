using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Queries.Invitations.ListInvitations;

public interface IListInvitationsQueryHandler : IQueryHandler<ListInvitationsQuery, IReadOnlyList<InvitationDto>>
{
}
