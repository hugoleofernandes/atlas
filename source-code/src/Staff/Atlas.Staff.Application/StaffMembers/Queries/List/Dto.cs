namespace Atlas.Staff.Application.StaffMembers.Queries.List;

public sealed record ListStaffMembersDto(
    Guid StaffMemberId,
    Guid? PartyId,
    string? EmployeeNumber,
    string? ContractType,
    string Status,
    string? HireDate
);
