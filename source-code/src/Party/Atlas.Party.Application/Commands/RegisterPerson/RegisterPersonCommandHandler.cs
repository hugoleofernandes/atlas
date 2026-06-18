using Atlas.Party.Application.Abstractions;
using Atlas.Party.Application.Repositories;
using Atlas.Party.Domain.Parties;
using Atlas.Party.Domain.Parties.Exceptions;
using Atlas.Party.Domain.Shared;
using Atlas.Party.Domain.Shared.Exceptions;
using Atlas.SharedKernel.Application;

namespace Atlas.Party.Application.Commands.RegisterPerson;

public sealed class RegisterPersonCommandHandler : IRegisterPersonCommandHandler
{
    private readonly IPersonRepository _personRepository;
    private readonly IRequestContext _requestContext;
    private readonly IPartyUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public RegisterPersonCommandHandler(
        IPersonRepository personRepository,
        IRequestContext requestContext,
        IPartyUnitOfWork uow
    )
    {
        _personRepository = personRepository;
        _requestContext = requestContext;
        _uow = uow;
    }

    public async Task<RegisterPersonOutput> ExecuteAsync(RegisterPersonCommand cmd, CancellationToken ct)
    {
        var tenantId = _requestContext.TenantId ?? throw new TenantContextNotResolvedException();

        var taxNumber = TaxNumber.Create(cmd.TaxNumber);

        if (await _personRepository.ExistsWithTaxNumberAsync(tenantId, taxNumber.Value, ct))
            throw new DuplicateTaxNumberException(taxNumber.Value);

        var name = PersonName.Create(cmd.FirstName, cmd.LastName, cmd.MiddleName);
        var person = Person.Register(tenantId, taxNumber, name, cmd.BirthDate, cmd.Gender, cmd.Notes);
        person.ReplaceAddresses(cmd.Addresses);
        person.ReplaceContacts(cmd.Contacts);
        person.ReplaceClassifications(cmd.Classifications);

        await _personRepository.AddAsync(person, ct);

        return new RegisterPersonOutput(person.Id, person.TaxNumber.Value, person.Name.FullName);
    }
}

