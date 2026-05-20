using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Queries.ListRoles;

public interface IListRolesQueryHandler : IQueryHandler<ListRolesQuery, PagedResult<RoleDto>>
{
}
