using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Roles.Handlers.Commands.RemoveRole;

public interface IRemoveRoleCommandHandler : ICommandHandler<RemoveRoleCommand, RemoveRoleOutput>
{
}
