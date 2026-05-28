using Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Queries.ListRoles;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Queries.GetRoleById;

public interface IGetRoleByIdQueryHandler : IQueryHandler<GetRoleByIdQuery, RoleDto?>
{
}
