using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Commands.RemoveRole;

public interface IRemoveRoleCommandHandler : ICommandHandler<RemoveRoleCommand, RemoveRoleOutput>
{
}
