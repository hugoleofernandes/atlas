using Atlas.BuildingBlocks.Persistence;
using Atlas.Identity.Application.Abstractions;

namespace Atlas.Identity.Infrastructure.Persistence.DbContexts;

public sealed class IdentityUnitOfWork : IIdentityUnitOfWork
{
    private readonly IdentityDbContext _db;
    private readonly ISavePipeline _savePipeline;

    public IdentityUnitOfWork(IdentityDbContext db, ISavePipeline savePipeline)
    {
        _db           = db;
        _savePipeline = savePipeline;
    }

    public Task SaveChangesAsync(CancellationToken ct)
        => _savePipeline.ExecuteAsync(_db, ct);
}
