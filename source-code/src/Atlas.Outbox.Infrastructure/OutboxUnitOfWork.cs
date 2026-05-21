using Atlas.SharedKernel.Application;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Outbox.Infrastructure;

/// <summary>
/// Minimal Unit of Work for the OutboxWorker — no audit, no stamping, no pipeline.
/// Just flushes the status updates (processed / failed / dead-lettered) for the current batch.
/// Each module registers it with its own DbContext so the save always targets the right schema.
/// </summary>
internal sealed class OutboxUnitOfWork : IUnitOfWork
{
    private readonly DbContext _db;

    public OutboxUnitOfWork(DbContext db) => _db = db;

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
