using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Commands.ActivateRole;

public interface IActivateRoleCommandHandler : ICommandHandler<ActivateRoleCommand, ActivateRoleOutput>
{
}
