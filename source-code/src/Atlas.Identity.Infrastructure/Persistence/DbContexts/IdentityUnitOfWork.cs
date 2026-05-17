using Atlas.BuildingBlocks.Persistence;
using Atlas.Identity.Application.Abstractions;
using Atlas.SharedKernel.Application.IntegrationEvents;

namespace Atlas.Identity.Infrastructure.Persistence.DbContexts;

public sealed class IdentityUnitOfWork : IIdentityUnitOfWork
{
    private readonly IdentityDbContext _db;
    private readonly IAuditService _auditService;
    private readonly IIntegrationEventEnqueuer _integrationEventEnqueuer;

    public IdentityUnitOfWork(
        IdentityDbContext db,
        IAuditService auditService,
        IIntegrationEventEnqueuer integrationEventEnqueuer)
    {
        _db = db;
        _auditService = auditService;
        _integrationEventEnqueuer = integrationEventEnqueuer;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        var domainEvents = _db.GetDomainEvents();

        await _integrationEventEnqueuer.EnqueueAsync(domainEvents, ct);
        await _auditService.AddAuditLogsAsync(_db, ct);
        await _db.SaveChangesAsync(ct);

        _db.ClearDomainEvents();
    }
}
