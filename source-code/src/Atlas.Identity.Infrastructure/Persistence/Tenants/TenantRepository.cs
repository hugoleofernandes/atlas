using Atlas.Identity.Application.Tenants.Abstractions;
using Atlas.Identity.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Persistence.Tenants;

public sealed class TenantRepository : ITenantRepository
{
    private readonly IdentityDbContext _db;

    public TenantRepository(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<Tenant?> GetBySlugWithUsersAndInvitationsAsync(
        string slug,
        CancellationToken ct)
    {
        return await _db.Tenants
            .Include(t => t.Users)
            .Include(t => t.Invitations)
            .FirstOrDefaultAsync(t => t.Slug == slug && t.IsActive, ct);
    }
}