using Atlas.BuildingBlocks.Persistence;
using Atlas.SharedKernel.Domain.Events;
using Atlas.Staff.Application.Abstractions;

namespace Atlas.Staff.Infrastructure.Persistence.DbContexts;

public sealed class StaffUnitOfWork : IStaffUnitOfWork
{
    private readonly StaffDbContext _db;
    private readonly IAuditService _auditService;

    public StaffUnitOfWork(StaffDbContext db, IAuditService auditService)
    {
        _db = db;
        _auditService = auditService;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _auditService.AddAuditLogsAsync(_db, ct);

        await _db.SaveChangesAsync(ct);

        _db.ClearDomainEvents();
    }

    public IEnumerable<IDomainEvent> GetDomainEvents()
    {
        return _db.GetDomainEvents();
    }
}
