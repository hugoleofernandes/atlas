using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Staff.Application.StaffMembers.Commands.Update;

public interface IUpdateStaffMemberCommandHandler
    : ICommandHandler<UpdateStaffMemberCommand, UpdateStaffMemberOutput>;
