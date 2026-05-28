using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Queries.ListRoles;

public interface IListRolesQueryHandler : IQueryHandler<ListRolesQuery, PagedResult<RoleDto>>
{
}
