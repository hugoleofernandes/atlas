using Atlas.Identity.Application.Tenants.Queries.Dtos;
using Atlas.Identity.Application.Tenants.Queries.ListRoles;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Entities.Tenants.Readers.ListRoles;

public sealed class ListRolesReader(IdentityDbContext db) : IListRolesReader
{
    public async Task<PagedResult<RoleDto>> ListAsync(
        Guid tenantId, int page, int pageSize, bool includeInactive, CancellationToken ct)
    {
        var query = db.Roles
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId && (includeInactive || r.IsActive))
            .OrderByDescending(r => r.IsSystem)
            .ThenBy(r => r.Name);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RoleDto(
                r.Id,
                r.Name,
                r.IsSystem,
                r.Permissions.Select(p => p.Code).ToList()))
            .ToListAsync(ct);

        return new PagedResult<RoleDto>(items, page, pageSize, total);
    }
}
