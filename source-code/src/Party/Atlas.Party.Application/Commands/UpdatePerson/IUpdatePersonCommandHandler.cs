using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Party.Application.Commands.UpdatePerson;

public interface IUpdatePersonCommandHandler : ICommandHandler<UpdatePersonCommand, UpdatePersonOutput>
{
}

