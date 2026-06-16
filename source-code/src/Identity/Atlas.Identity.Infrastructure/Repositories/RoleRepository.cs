using Atlas.Identity.Application.Repositories;
using Atlas.Identity.Domain.Roles;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly IdentityDbContext _db;

    public RoleRepository(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<Role?> GetByIdWithPermissionsAsync(Guid roleId, CancellationToken ct)
    {
        return await _db.Roles.Include(r => r.Permissions).FirstOrDefaultAsync(r => r.Id == roleId, ct);
    }

    public async Task<IReadOnlyList<Role>> GetByTenantIdWithPermissionsAsync(Guid tenantId, CancellationToken ct)
    {
        return await _db.Roles.Include(r => r.Permissions).Where(r => r.TenantId == tenantId).ToListAsync(ct);
    }

    public async Task<bool> ExistsWithNameAsync(Guid tenantId, string name, CancellationToken ct)
    {
        return await _db.Roles.AnyAsync(r => r.TenantId == tenantId && r.Name == name && r.IsActive, ct);
    }

    public async Task<bool> ExistsWithNameExcludingAsync(
        Guid tenantId,
        string name,
        Guid excludeRoleId,
        CancellationToken ct
    )
    {
        return await _db.Roles.AnyAsync(r => r.TenantId == tenantId && r.Name == name && r.Id != excludeRoleId, ct);
    }

    public async Task AddAsync(Role role, CancellationToken ct)
    {
        await _db.Roles.AddAsync(role, ct);
    }

    public void Remove(Role role)
    {
        _db.Roles.Remove(role);
    }
}
