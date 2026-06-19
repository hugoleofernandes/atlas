using Atlas.BuildingBlocks.Audit.Queries;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Party.Application.Queries.Audit.ListEntries;

public interface IPartyListAuditEntriesQueryHandler
    : IQueryHandler<ListAuditEntriesQuery, IReadOnlyList<AuditEntryDto>>;
