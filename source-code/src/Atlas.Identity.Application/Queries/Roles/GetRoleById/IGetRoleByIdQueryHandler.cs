using Atlas.Identity.Application.Queries.Roles.ListRoles;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Queries.Roles.GetRoleById;

public interface IGetRoleByIdQueryHandler : IQueryHandler<GetRoleByIdQuery, RoleDto?>
{
}
