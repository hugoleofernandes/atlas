namespace Atlas.Staff.Application.StaffMembers.Queries.GetByPartyId;

public sealed record GetStaffMemberByPartyIdDto(
    Guid StaffMemberId,
    string Status
);
