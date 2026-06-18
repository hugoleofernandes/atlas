using Atlas.Staff.Domain.Shared;

namespace Atlas.Staff.Application.StaffMembers.Queries.GetById;

public sealed record GetStaffMemberByIdDto(
    Guid StaffMemberId,
    Guid TenantId,
    Guid? PartyId,
    string? EmployeeNumber,
    string? ContractType,
    string? HireDate,
    string? TerminationDate,
    string Status,
    DateTime CreatedAt
);
