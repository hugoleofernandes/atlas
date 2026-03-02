using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Infrastructure.Persistence;

public sealed class IdentityUnitOfWork : IUnitOfWork
{
    private readonly IdentityDbContext _db;

    public IdentityUnitOfWork(IdentityDbContext db)
    {
        _db = db;
    }

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}