using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Queries.GetRoleById;

public interface IGetRoleByIdQueryHandler : IQueryHandler<GetRoleByIdQuery, RoleDto?>
{
}
