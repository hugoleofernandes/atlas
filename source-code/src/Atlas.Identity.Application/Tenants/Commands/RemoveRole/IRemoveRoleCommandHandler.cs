using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Commands.RemoveRole;

public interface IRemoveRoleCommandHandler : ICommandHandler<RemoveRoleCommand, RemoveRoleOutput>
{
}
