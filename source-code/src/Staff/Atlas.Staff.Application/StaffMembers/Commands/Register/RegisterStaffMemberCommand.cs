using Atlas.Staff.Domain.Shared;

namespace Atlas.Staff.Application.StaffMembers.Commands.Register;

public sealed record RegisterStaffMemberCommand(
    Guid PartyId,
    string EmployeeNumber,
    ContractType ContractType,
    DateOnly HireDate
);
