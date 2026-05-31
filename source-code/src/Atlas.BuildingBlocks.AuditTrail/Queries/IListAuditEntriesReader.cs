namespace Atlas.BuildingBlocks.AuditTrail.Queries;

public interface IListAuditEntriesReader
{
    Task<IReadOnlyList<AuditEntryDto>> ListAsync(
        ListAuditEntriesQuery query,
        Guid                  tenantId,
        CancellationToken     ct);
}
