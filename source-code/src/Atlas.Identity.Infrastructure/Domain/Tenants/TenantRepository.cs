using Atlas.Identity.Application.Tenants;
using Atlas.Identity.Domain.Tenants;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Domain.Tenants;

public sealed class TenantRepository : ITenantRepository
{
    private readonly IdentityDbContext _db;

    public TenantRepository(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<Tenant?> GetByIdWithRolesAsync(
        Guid id,
        CancellationToken ct)
    {
        return await _db.Tenants
            .Include(t => t.Roles)
                .ThenInclude(r => r.Permissions)
            .AsSplitQuery()
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive, ct);
    }

    public async Task<Tenant?> GetByNameWithRolesAsync(
        string name,
        CancellationToken ct)
    {
        return await _db.Tenants
            .Include(t => t.Roles)
                .ThenInclude(r => r.Permissions)
            .AsSplitQuery()
            .FirstOrDefaultAsync(t => t.Name == name && t.IsActive, ct);
    }
}
