using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Queries.GetRoleById;
using Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Queries.ListRoles;

namespace Atlas.Identity.Infrastructure.Aggregates.Tenants._Roles.Readers.GetRoleById;

public sealed class GetRoleByIdReader(IdentityDbContext db) : IGetRoleByIdReader
{
    public Task<RoleDto?> GetByIdAsync(Guid tenantId, Guid roleId, CancellationToken ct)
    {
        return db.Roles
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.Id == roleId)
            .Select(r => new RoleDto(
                r.Id,
                r.Name,
                r.IsSystem,
                r.Permissions.Select(p => p.Code).ToList()))
            .FirstOrDefaultAsync(ct);
    }
}
