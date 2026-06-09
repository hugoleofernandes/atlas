using Atlas.BuildingBlocks.Audit.Queries;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Staff.Application.StaffMembers.Queries.Audit.ListEntries;

public interface IStaffListAuditEntriesQueryHandler
    : IQueryHandler<ListAuditEntriesQuery, IReadOnlyList<AuditEntryDto>>;
