using Atlas.Identity.Application.Users;
using Atlas.Identity.Domain.Shared;
using Atlas.Identity.Domain.Users;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Identity.Infrastructure.Domain.Users;

public sealed class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _db;

    public UserRepository(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<User?> FindActiveByEmailAsync(Guid tenantId, Email email, CancellationToken ct)
    {
        return await _db.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenantId
                                   && u.Email == email
                                   && u.IsActive, ct);
    }

    public async Task<bool> ExistsWithEmailAsync(Guid tenantId, Email email, CancellationToken ct)
    {
        return await _db.Users
            .AnyAsync(u => u.TenantId == tenantId && u.Email == email, ct);
    }

    public async Task<bool> HasActiveWithRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct)
    {
        return await _db.Users
            .AnyAsync(u => u.TenantId == tenantId && u.RoleId == roleId && u.IsActive, ct);
    }

    public async Task<bool> HasAnyWithRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct)
    {
        return await _db.Users
            .AnyAsync(u => u.TenantId == tenantId && u.RoleId == roleId, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await _db.Users.AddAsync(user, ct);
    }
}
