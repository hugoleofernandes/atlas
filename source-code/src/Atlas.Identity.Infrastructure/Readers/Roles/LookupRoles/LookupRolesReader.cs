using Atlas.Identity.Application.Queries.Roles.LookupRoles;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Readers.Roles.LookupRoles;

public sealed class LookupRolesReader(IdentityDbContext db) : ILookupRolesReader
{
    public async Task<IReadOnlyList<RoleLookupDto>> LookupAsync(Guid tenantId, CancellationToken ct)
    {
        return await db.Roles
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.IsActive)
            .OrderByDescending(r => r.IsSystem)
            .ThenBy(r => r.Name)
            .Select(r => new RoleLookupDto(r.Id, r.Name))
            .ToListAsync(ct);
    }
}
