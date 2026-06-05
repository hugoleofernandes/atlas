using Atlas.BuildingBlocks.Audit.Queries;
using Atlas.SharedKernel.Application;

namespace Atlas.Staff.Application.StaffMembers.Queries.Audit.ListEntries;

public sealed class StaffListAuditEntriesQueryHandler(
    IListAuditEntriesReader reader,
    IRequestContext context)
    : ListAuditEntriesQueryHandler(reader, context), IStaffListAuditEntriesQueryHandler;
