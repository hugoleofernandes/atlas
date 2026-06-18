using Atlas.Staff.Domain.Shared;

namespace Atlas.Staff.BffApi.Endpoints.StaffMembers.Update;

public sealed record UpdateStaffMemberRequest(
    ContractType ContractType,
    DateOnly HireDate
);
