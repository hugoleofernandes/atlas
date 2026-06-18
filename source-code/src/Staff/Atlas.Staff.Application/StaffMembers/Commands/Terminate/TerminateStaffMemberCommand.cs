namespace Atlas.Staff.Application.StaffMembers.Commands.Terminate;

public sealed record TerminateStaffMemberCommand(
    Guid StaffMemberId,
    DateOnly TerminationDate
);
