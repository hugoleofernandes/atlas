using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Tenants.Roles.Handlers.Queries.ListRoles;

public interface IListRolesReader
{
    Task<PagedResult<RoleDto>> ListAsync(Guid tenantId, int page, int pageSize, bool includeInactive, CancellationToken ct);
}
