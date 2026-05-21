using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Tenants.Queries.ListRoles;

public interface IListRolesReader
{
    Task<PagedResult<RoleDto>> ListAsync(Guid tenantId, int page, int pageSize, bool includeInactive, CancellationToken ct);
}
