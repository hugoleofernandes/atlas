using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Tenants.Roles.Handlers.Commands.CreateRole;

public interface ICreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, CreateRoleOutput>
{
}
