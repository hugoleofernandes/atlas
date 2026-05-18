using Atlas.Identity.Application.Tenants.Repositories;
using Atlas.Identity.Domain.Entities.Tenants;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Entities.Tenants.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly IdentityDbContext _db;

    public TenantRepository(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<Tenant?> GetByNameWithUsersInvitationsAndRolesAsync(
        string name,
        CancellationToken ct)
    {
        return await _db.Tenants
            .Include(t => t.Roles)
                .ThenInclude(r => r.Permissions)
            .Include(t => t.Users)
            .Include(t => t.Invitations)
            .FirstOrDefaultAsync(t => t.Name == name && t.IsActive, ct);
    }
}
