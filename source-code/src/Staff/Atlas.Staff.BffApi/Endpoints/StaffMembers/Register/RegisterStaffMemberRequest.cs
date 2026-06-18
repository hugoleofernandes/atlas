using Atlas.Staff.Domain.Shared;

namespace Atlas.Staff.BffApi.Endpoints.StaffMembers.Register;

public sealed record RegisterStaffMemberRequest(
    Guid PartyId,
    string EmployeeNumber,
    ContractType ContractType,
    DateOnly HireDate
);
