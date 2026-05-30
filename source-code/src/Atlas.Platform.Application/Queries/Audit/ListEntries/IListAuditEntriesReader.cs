namespace Atlas.Platform.Application.Queries.Audit.ListEntries;

public interface IListAuditEntriesReader
{
    Task<IReadOnlyList<AuditEntryDto>> ListAsync(
        ListAuditEntriesQuery query,
        Guid                  tenantId,
        CancellationToken     ct);
}
