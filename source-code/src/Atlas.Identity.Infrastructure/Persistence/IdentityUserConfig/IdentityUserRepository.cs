using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace Atlas.Identity.Infrastructure.Persistence.IdentityUserConfig;

public sealed class IdentityUserRepository : IIdentityUserRepository
{
    private readonly IdentityDbContext _db;

    public IdentityUserRepository(IdentityDbContext db)
    {
        _db = db;
    }

    public Task<IdentityUser?> GetByExternalIdAsync(
        string externalId,
        CancellationToken ct)
    {
        return _db.IdentityUsers
            .FirstOrDefaultAsync(x =>
                x.ExternalId == externalId && x.IsActive, ct);
    }

    public Task<IdentityUser?> GetByIdAsync(
        Guid id,
        CancellationToken ct)
    {
        return _db.IdentityUsers
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task AddAsync(
        IdentityUser user,
        CancellationToken ct)
    {
        await _db.IdentityUsers.AddAsync(user, ct);
    }
}