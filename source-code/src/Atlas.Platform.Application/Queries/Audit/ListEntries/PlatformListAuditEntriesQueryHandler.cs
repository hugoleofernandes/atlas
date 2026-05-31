using Atlas.BuildingBlocks.AuditTrail.Queries;
using Atlas.SharedKernel.Application;

namespace Atlas.Platform.Application.Queries.Audit.ListEntries;

public sealed class PlatformListAuditEntriesQueryHandler(
    IListAuditEntriesReader reader,
    IRequestContext context)
    : ListAuditEntriesQueryHandler(reader, context), IPlatformListAuditEntriesQueryHandler;
