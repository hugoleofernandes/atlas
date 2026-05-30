using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Platform.Application.Queries.Audit.ListEntries;

public interface IListAuditEntriesQueryHandler
    : IQueryHandler<ListAuditEntriesQuery, IReadOnlyList<AuditEntryDto>>;
