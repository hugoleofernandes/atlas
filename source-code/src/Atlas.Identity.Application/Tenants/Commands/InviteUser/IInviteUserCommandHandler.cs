using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Commands.InviteUser;

public interface IInviteUserCommandHandler : ICommandHandler<InviteUserCommand, InviteUserOutput>
{
}
