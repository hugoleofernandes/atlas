using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Commands.CreateRole;

public interface ICreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, CreateRoleOutput>
{
}
