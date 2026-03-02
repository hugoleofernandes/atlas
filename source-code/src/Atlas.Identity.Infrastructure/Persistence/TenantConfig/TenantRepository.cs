using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Persistence.TenantConfig;

public sealed class TenantRepository : ITenantRepository
{
    private readonly IdentityDbContext _db;

    public TenantRepository(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<Tenant?> GetBySlugWithMembershipsAsync(
        string slug,
        CancellationToken ct)
    {
        return await _db.Tenants
            .Include(t => t.Memberships)
            .FirstOrDefaultAsync(t => t.Slug == slug && t.IsActive, ct);
    }
}