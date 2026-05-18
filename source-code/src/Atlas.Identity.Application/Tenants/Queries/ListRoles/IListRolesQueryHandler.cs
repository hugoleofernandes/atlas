using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Tenants.Queries.ListRoles;

public interface IListRolesQueryHandler
{
    Task<PagedResult<RoleDto>> ExecuteAsync(Query query, CancellationToken ct);
}
