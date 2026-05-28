using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Roles.Handlers.Commands.UpdateRole;

public interface IUpdateRoleCommandHandler : ICommandHandler<UpdateRoleCommand, UpdateRoleOutput>
{
}
