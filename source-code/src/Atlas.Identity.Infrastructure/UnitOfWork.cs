using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AtlasDbContext _db;

    public UnitOfWork(AtlasDbContext db)
    {
        _db = db;
    }

    public Task SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}