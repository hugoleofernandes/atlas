using Atlas.Identity.Application.Tenants.Queries.Dtos;

namespace Atlas.Identity.Application.Tenants.Queries.GetRoleById;

public interface IGetRoleByIdReader
{
    Task<RoleDto?> GetByIdAsync(Guid tenantId, Guid roleId, CancellationToken ct);
}
