using Atlas.Party.Application.Abstractions;
using Atlas.Party.Application.Repositories;
using Atlas.Party.Domain.Parties.Exceptions;
using Atlas.Party.Domain.Shared;
using Atlas.SharedKernel.Application;

namespace Atlas.Party.Application.Commands.UpdatePerson;

public sealed class UpdatePersonCommandHandler : IUpdatePersonCommandHandler
{
    private readonly IPersonRepository _personRepository;
    private readonly IPartyUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public UpdatePersonCommandHandler(IPersonRepository personRepository, IPartyUnitOfWork uow)
    {
        _personRepository = personRepository;
        _uow = uow;
    }

    public async Task<UpdatePersonOutput> ExecuteAsync(UpdatePersonCommand cmd, CancellationToken ct)
    {
        var person =
            await _personRepository.GetByIdAsync(cmd.PartyId, ct)
            ?? throw new PersonNotFoundException(cmd.PartyId);

        var name = PersonName.Create(cmd.FirstName, cmd.LastName, cmd.MiddleName);
        person.Update(name, cmd.BirthDate, cmd.Gender, cmd.Notes);
        person.ReplaceAddresses(cmd.Addresses);
        person.ReplaceContacts(cmd.Contacts);
        person.ReplaceClassifications(cmd.Classifications);

        return new UpdatePersonOutput(person.Id, person.Name.FullName);
    }
}

