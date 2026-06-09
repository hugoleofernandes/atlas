using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Staff.Application.StaffMembers.Commands.CreateFromInvitation;

/// <summary>
/// Creates a StaffMember when a user accepts an invitation.
/// Implements <see cref="ICommandHandler{TCommand,TOutput}"/> so the standard
/// pipeline runs: idempotency guard → validation → handler → UoW save.
/// </summary>
public interface ICreateStaffMemberFromInvitationCommandHandler
    : ICommandHandler<CreateStaffMemberFromInvitationCommand, Unit>
{
}
