using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Identity.Application.Aggregates.Tenants._Roles.Handlers.Commands.CreateRole;

public interface ICreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, CreateRoleOutput>
{
}
