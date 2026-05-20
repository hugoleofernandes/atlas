using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Commands.UpdateRole;

public interface IUpdateRoleCommandHandler : ICommandHandler<UpdateRoleCommand, UpdateRoleOutput>
{
}
