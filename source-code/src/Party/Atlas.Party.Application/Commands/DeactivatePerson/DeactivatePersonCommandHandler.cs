using Atlas.Party.Application.Abstractions;
using Atlas.Party.Application.Repositories;
using Atlas.Party.Domain.Parties.Exceptions;
using Atlas.SharedKernel.Application;

namespace Atlas.Party.Application.Commands.DeactivatePerson;

public sealed class DeactivatePersonCommandHandler : IDeactivatePersonCommandHandler
{
    private readonly IPersonRepository _personRepository;
    private readonly IPartyUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public DeactivatePersonCommandHandler(IPersonRepository personRepository, IPartyUnitOfWork uow)
    {
        _personRepository = personRepository;
        _uow = uow;
    }

    public async Task<DeactivatePersonOutput> ExecuteAsync(DeactivatePersonCommand cmd, CancellationToken ct)
    {
        var person =
            await _personRepository.GetByIdAsync(cmd.PartyId, ct)
            ?? throw new PersonNotFoundException(cmd.PartyId);

        person.Deactivate();

        return new DeactivatePersonOutput(person.Id, person.IsActive);
    }
}

