using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Staff.Application.StaffMembers.Commands.Terminate;

public interface ITerminateStaffMemberCommandHandler
    : ICommandHandler<TerminateStaffMemberCommand, Unit>;
