namespace Atlas.Staff.Application.StaffMembers.Commands.Create;

public sealed record Command(
    Guid TenantId,
    Guid UserId,
    string FirstName,
    string LastName,
    string Role
);