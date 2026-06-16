using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Party.Application.Commands.UpdateOrganization;

public interface IUpdateOrganizationCommandHandler : ICommandHandler<UpdateOrganizationCommand, UpdateOrganizationOutput>
{
}
