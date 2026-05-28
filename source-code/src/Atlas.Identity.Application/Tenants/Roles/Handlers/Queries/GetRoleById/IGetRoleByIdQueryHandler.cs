using Atlas.Identity.Application.Tenants.Roles.Handlers.Queries.ListRoles;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Roles.Handlers.Queries.GetRoleById;

public interface IGetRoleByIdQueryHandler : IQueryHandler<GetRoleByIdQuery, RoleDto?>
{
}
