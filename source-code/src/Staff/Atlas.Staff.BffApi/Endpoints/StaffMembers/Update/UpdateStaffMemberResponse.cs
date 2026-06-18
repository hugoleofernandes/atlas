using Atlas.Staff.Application.StaffMembers.Commands.Update;

namespace Atlas.Staff.BffApi.Endpoints.StaffMembers.Update;

public sealed record UpdateStaffMemberResponse(Guid StaffMemberId)
{
    public static UpdateStaffMemberResponse From(UpdateStaffMemberOutput output)
        => new(output.StaffMemberId);
}
