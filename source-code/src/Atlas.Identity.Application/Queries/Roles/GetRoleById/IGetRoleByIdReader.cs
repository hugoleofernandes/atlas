using Atlas.Identity.Application.Queries.Roles.ListRoles;

namespace Atlas.Identity.Application.Queries.Roles.GetRoleById;

public interface IGetRoleByIdReader
{
    Task<RoleDto?> GetByIdAsync(Guid tenantId, Guid roleId, CancellationToken ct);
}
