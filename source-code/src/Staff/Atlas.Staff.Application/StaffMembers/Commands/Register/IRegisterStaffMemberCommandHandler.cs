using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Staff.Application.StaffMembers.Commands.Register;

public interface IRegisterStaffMemberCommandHandler
    : ICommandHandler<RegisterStaffMemberCommand, RegisterStaffMemberOutput>;
