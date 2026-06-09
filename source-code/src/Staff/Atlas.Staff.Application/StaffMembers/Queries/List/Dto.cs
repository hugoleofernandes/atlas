namespace Atlas.Staff.Application.StaffMembers.Queries.List;

public sealed record Dto(
    Guid Id,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive
);