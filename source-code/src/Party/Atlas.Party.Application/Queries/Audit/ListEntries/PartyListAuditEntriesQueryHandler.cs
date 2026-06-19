using Atlas.BuildingBlocks.Audit.Queries;
using Atlas.SharedKernel.Application;

namespace Atlas.Party.Application.Queries.Audit.ListEntries;

public sealed class PartyListAuditEntriesQueryHandler(
    IListAuditEntriesReader reader,
    IRequestContext context)
    : ListAuditEntriesQueryHandler(reader, context), IPartyListAuditEntriesQueryHandler;
