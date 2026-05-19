using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Tenants.Queries.ListRoles;

public interface IListRolesQueryHandler
{
    Task<PagedResult<RoleDto>> ExecuteAsync(ListRolesQuery query, CancellationToken ct);
}
