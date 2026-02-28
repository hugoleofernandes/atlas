using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Domain.Entities;
using Atlas.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public sealed class TenantRepository : ITenantRepository
{
    private readonly AtlasDbContext _db;

    public TenantRepository(AtlasDbContext db)
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