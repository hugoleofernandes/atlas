using Atlas.BuildingBlocks.Audit.Queries;
using Atlas.SharedKernel.Application;

namespace Atlas.Identity.Application.Queries.Audit.ListEntries;

public sealed class IdentityListAuditEntriesQueryHandler(
    IListAuditEntriesReader reader,
    IRequestContext context)
    : ListAuditEntriesQueryHandler(reader, context), IIdentityListAuditEntriesQueryHandler;
