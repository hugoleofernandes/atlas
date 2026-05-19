using Atlas.Identity.Application.Tenants.Queries.GetRoleById;
using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Entities.Tenants.Queries.GetRoleById;

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
