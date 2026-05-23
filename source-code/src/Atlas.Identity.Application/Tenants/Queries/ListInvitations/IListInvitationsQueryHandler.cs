using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Queries.ListInvitations;

public interface IListInvitationsQueryHandler : IQueryHandler<ListInvitationsQuery, PagedResult<InvitationDto>>
{
}
