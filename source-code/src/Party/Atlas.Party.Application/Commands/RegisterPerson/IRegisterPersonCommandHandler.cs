using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Party.Application.Commands.RegisterPerson;

public interface IRegisterPersonCommandHandler : ICommandHandler<RegisterPersonCommand, RegisterPersonOutput>
{
}

