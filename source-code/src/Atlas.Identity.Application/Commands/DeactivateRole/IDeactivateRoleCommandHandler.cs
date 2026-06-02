using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Commands.DeactivateRole;

public interface IDeactivateRoleCommandHandler : ICommandHandler<DeactivateRoleCommand, DeactivateRoleOutput>
{
}
