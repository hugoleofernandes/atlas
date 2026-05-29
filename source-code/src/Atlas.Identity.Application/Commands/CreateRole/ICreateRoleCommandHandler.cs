using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Commands.CreateRole;

public interface ICreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, CreateRoleOutput>
{
}
