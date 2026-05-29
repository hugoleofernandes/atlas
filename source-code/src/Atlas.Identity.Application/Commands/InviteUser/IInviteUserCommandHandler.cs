using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Commands.InviteUser;

public interface IInviteUserCommandHandler : ICommandHandler<InviteUserCommand, InviteUserOutput>
{
}
