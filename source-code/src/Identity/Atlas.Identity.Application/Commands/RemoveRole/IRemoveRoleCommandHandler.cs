using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Commands.RemoveRole;

public interface IRemoveRoleCommandHandler : ICommandHandler<RemoveRoleCommand, RemoveRoleOutput>
{
}
