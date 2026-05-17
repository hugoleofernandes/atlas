using Atlas.BuildingBlocks.Persistence;
using Atlas.Identity.Application.Abstractions;
using Atlas.SharedKernel.Domain.Events;

namespace Atlas.Identity.Infrastructure.Persistence.DbContexts;

public sealed class IdentityUnitOfWork : IIdentityUnitOfWork
{
    private readonly IdentityDbContext _db;
    private readonly IAuditService _auditService;

    public IdentityUnitOfWork(IdentityDbContext db, IAuditService auditService)
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
