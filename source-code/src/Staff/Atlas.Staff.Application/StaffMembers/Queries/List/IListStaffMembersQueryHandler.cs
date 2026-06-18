using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Staff.Application.StaffMembers.Queries.List;

public interface IListStaffMembersQueryHandler
    : IQueryHandler<ListStaffMembersQuery, IReadOnlyList<ListStaffMembersDto>>;
