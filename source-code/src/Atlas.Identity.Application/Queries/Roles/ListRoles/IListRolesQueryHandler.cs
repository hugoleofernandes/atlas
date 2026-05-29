using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Queries.Roles.ListRoles;

public interface IListRolesQueryHandler : IQueryHandler<ListRolesQuery, IReadOnlyList<RoleDto>>
{
}
