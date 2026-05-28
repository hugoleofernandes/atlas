using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Commands.UpdateRole;

public interface IUpdateRoleCommandHandler : ICommandHandler<UpdateRoleCommand, UpdateRoleOutput>
{
}
