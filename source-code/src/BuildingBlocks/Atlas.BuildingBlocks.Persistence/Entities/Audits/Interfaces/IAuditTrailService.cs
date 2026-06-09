using Microsoft.EntityFrameworkCore;

namespace Atlas.BuildingBlocks.Persistence.Entities.Audits.Interfaces;

public interface IAuditTrailService
{
    Task RecordAsync(DbContext db, CancellationToken ct);
}
