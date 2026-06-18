using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Staff.Application.StaffMembers.Queries.GetById;

public interface IGetStaffMemberByIdQueryHandler
    : IQueryHandler<GetStaffMemberByIdQuery, GetStaffMemberByIdDto>;
