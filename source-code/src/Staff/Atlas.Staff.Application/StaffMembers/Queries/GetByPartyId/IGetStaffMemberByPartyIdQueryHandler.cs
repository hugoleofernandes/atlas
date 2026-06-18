using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Staff.Application.StaffMembers.Queries.GetByPartyId;

public interface IGetStaffMemberByPartyIdQueryHandler
    : IQueryHandler<GetStaffMemberByPartyIdQuery, GetStaffMemberByPartyIdDto?>;
