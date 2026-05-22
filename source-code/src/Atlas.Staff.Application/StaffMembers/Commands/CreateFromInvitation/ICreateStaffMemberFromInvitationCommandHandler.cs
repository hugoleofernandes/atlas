using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Staff.Application.StaffMembers.Commands.CreateFromInvitation;

/// <summary>
/// Extends <see cref="IHandler{TInput,TOutput}"/> (not ICommandHandler) because
/// the handler manages its own UnitOfWork internally — no external save pipeline needed.
/// </summary>
public interface ICreateStaffMemberFromInvitationCommandHandler
    : IHandler<CreateStaffMemberFromInvitationCommand, Unit>
{
}
