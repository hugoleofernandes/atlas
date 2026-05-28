using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Invitations.Handlers.Commands.InviteUser;

public interface IInviteUserCommandHandler : ICommandHandler<InviteUserCommand, InviteUserOutput>
{
}
