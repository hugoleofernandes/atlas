using Microsoft.EntityFrameworkCore;

namespace Atlas.BuildingBlocks.Persistence;

public interface IAuditTrailService
{
    Task RecordAsync(DbContext db, CancellationToken ct);
}
