using Atlas.BuildingBlocks.Persistence;
using Atlas.SharedKernel.Domain.Events;
using Atlas.Staff.Application.Abstractions;

namespace Atlas.Staff.Infrastructure.Persistence.DbContexts;

public sealed class StaffUnitOfWork : UnitOfWorkBase, IStaffUnitOfWork
{
    private readonly StaffDbContext _db;
    private readonly ISavePipeline _savePipeline;

    public StaffUnitOfWork(StaffDbContext db, ISavePipeline savePipeline)
    {
        _db = db;
        _savePipeline = savePipeline;
    }

    public Task SaveChangesAsync(CancellationToken ct) => ExecuteSaveAsync(ct);

    public IEnumerable<IDomainEvent> GetDomainEvents() => _db.GetDomainEvents();

    protected override async Task CommitAsync(CancellationToken ct)
    {
        await _savePipeline.ExecuteAsync(_db, ct);
        await _db.SaveChangesAsync(ct);
        _db.ClearDomainEvents();
    }
}
