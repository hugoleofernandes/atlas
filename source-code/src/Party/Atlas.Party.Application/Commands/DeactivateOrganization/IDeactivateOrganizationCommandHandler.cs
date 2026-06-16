using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Party.Application.Commands.DeactivateOrganization;

public interface IDeactivateOrganizationCommandHandler
    : ICommandHandler<DeactivateOrganizationCommand, DeactivateOrganizationOutput>
{
}
