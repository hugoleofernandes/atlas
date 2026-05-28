using Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Queries.ListRoles;

namespace Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Queries.GetRoleById;

public interface IGetRoleByIdReader
{
    Task<RoleDto?> GetByIdAsync(Guid tenantId, Guid roleId, CancellationToken ct);
}
