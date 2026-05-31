using Atlas.BuildingBlocks.AuditTrail.Queries;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Platform.Application.Queries.Audit.ListEntries;

public interface IPlatformListAuditEntriesQueryHandler
    : IQueryHandler<ListAuditEntriesQuery, IReadOnlyList<AuditEntryDto>>;
