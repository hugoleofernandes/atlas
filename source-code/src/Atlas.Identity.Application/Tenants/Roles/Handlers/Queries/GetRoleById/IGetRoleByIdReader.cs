using Atlas.Identity.Application.Tenants.Roles.Handlers.Queries.ListRoles;

namespace Atlas.Identity.Application.Tenants.Roles.Handlers.Queries.GetRoleById;

public interface IGetRoleByIdReader
{
    Task<RoleDto?> GetByIdAsync(Guid tenantId, Guid roleId, CancellationToken ct);
}
