using Atlas.Identity.Application.Users.Abstractions;
using Atlas.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;


namespace Atlas.Identity.Infrastructure.Persistence.Users;

public sealed class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _db;

    public UserRepository(IdentityDbContext db)
    {
        _db = db;
    }

    public Task<User?> GetByExternalIdAsync(
        string externalId,
        CancellationToken ct)
    {
        return _db.IdentityUsers
            .FirstOrDefaultAsync(x =>
                x.ExternalId == externalId, ct);
    }

    public Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken ct)
    {
        return _db.IdentityUsers
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task AddAsync(
        User user,
        CancellationToken ct)
    {
        await _db.IdentityUsers.AddAsync(user, ct);
    }
}