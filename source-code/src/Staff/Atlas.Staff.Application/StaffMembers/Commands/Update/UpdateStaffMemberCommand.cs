using Atlas.Staff.Domain.Shared;

namespace Atlas.Staff.Application.StaffMembers.Commands.Update;

public sealed record UpdateStaffMemberCommand(
    Guid StaffMemberId,
    ContractType ContractType,
    DateOnly HireDate
);
