using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Commands.UpdateRole;

public interface IUpdateRoleCommandHandler : ICommandHandler<UpdateRoleCommand, UpdateRoleOutput>
{
}
