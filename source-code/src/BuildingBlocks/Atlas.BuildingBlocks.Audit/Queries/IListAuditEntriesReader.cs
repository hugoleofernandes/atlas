namespace Atlas.BuildingBlocks.Audit.Queries;

public interface IListAuditEntriesReader
{
    Task<IReadOnlyList<AuditEntryDto>> ListAsync(
        ListAuditEntriesQuery query,
        Guid                  tenantId,
        CancellationToken     ct);
}
