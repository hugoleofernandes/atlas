using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Party.Application.Commands.RegisterOrganization;

public interface IRegisterOrganizationCommandHandler
    : ICommandHandler<RegisterOrganizationCommand, RegisterOrganizationOutput>
{
}
