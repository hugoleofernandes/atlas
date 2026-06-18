using Atlas.Staff.Application.StaffMembers.Commands.Register;

namespace Atlas.Staff.BffApi.Endpoints.StaffMembers.Register;

public sealed record RegisterStaffMemberResponse(Guid StaffMemberId)
{
    public static RegisterStaffMemberResponse From(RegisterStaffMemberOutput output)
        => new(output.StaffMemberId);
}
