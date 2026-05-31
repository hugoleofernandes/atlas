using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.BuildingBlocks.AuditTrail.Queries;

/// <summary>
/// Base query handler for listing audit entries.
/// Each module subclasses this via a marker interface (e.g. IIdentityListAuditEntriesQueryHandler)
/// and wires in the module-specific concrete reader through a factory lambda in DI —
/// avoiding the registration conflict that occurs when every module registers IListAuditEntriesReader.
/// </summary>
public class ListAuditEntriesQueryHandler : IQueryHandler<ListAuditEntriesQuery, IReadOnlyList<AuditEntryDto>>
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
