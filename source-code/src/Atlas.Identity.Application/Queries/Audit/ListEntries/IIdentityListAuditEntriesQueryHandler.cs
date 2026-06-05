using Atlas.BuildingBlocks.Audit.Queries;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Queries.Audit.ListEntries;

public interface IIdentityListAuditEntriesQueryHandler
    : IQueryHandler<ListAuditEntriesQuery, IReadOnlyList<AuditEntryDto>>;
