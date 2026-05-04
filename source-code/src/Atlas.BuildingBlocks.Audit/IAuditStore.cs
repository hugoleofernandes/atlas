namespace Atlas.BuildingBlocks.Audit;

public interface IAuditStore
{
    Task AddAsync(AuditEntry entry, CancellationToken ct);
}