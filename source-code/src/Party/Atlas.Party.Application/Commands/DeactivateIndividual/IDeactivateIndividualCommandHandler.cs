using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Party.Application.Commands.DeactivateIndividual;

public interface IDeactivateIndividualCommandHandler
    : ICommandHandler<DeactivateIndividualCommand, DeactivateIndividualOutput>
{
}
