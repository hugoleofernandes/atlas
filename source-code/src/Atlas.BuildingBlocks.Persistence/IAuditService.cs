using Microsoft.EntityFrameworkCore;

namespace Atlas.BuildingBlocks.Persistence;

public interface IAuditService
{
    Task AddAuditLogsAsync(DbContext db, CancellationToken ct);
}
