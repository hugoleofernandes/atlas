using Atlas.BuildingBlocks.Persistence;
using Atlas.Identity.Application.Abstractions;

namespace Atlas.Identity.Infrastructure.Persistence.DbContexts;

public sealed class IdentityUnitOfWork : IIdentityUnitOfWork
{
    private readonly IdentityDbContext _db;
    private readonly ISavePipeline _savePipeline;

    public IdentityUnitOfWork(IdentityDbContext db, ISavePipeline savePipeline)
    {
        _db = db;
        _savePipeline = savePipeline;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _savePipeline.ExecuteAsync(_db, ct);
        await _db.SaveChangesAsync(ct);
        _db.ClearDomainEvents();
    }
}
