using Atlas.SharedKernel.Application;

namespace Atlas.Platform.Application.Queries.Audit.ListEntries;

public sealed class ListAuditEntriesQueryHandler : IListAuditEntriesQueryHandler
{
    private readonly IListAuditEntriesReader _reader;
    private readonly IRequestContext         _context;

    public ListAuditEntriesQueryHandler(IListAuditEntriesReader reader, IRequestContext context)
    {
        _reader  = reader;
        _context = context;
    }

    public Task<IReadOnlyList<AuditEntryDto>> ExecuteAsync(ListAuditEntriesQuery query, CancellationToken ct)
    {
        var tenantId = _context.TenantId!.Value;
        return _reader.ListAsync(query, tenantId, ct);
    }
}
