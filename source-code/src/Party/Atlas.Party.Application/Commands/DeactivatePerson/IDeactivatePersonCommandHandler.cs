using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Party.Application.Commands.DeactivatePerson;

public interface IDeactivatePersonCommandHandler
    : ICommandHandler<DeactivatePersonCommand, DeactivatePersonOutput>
{
}

