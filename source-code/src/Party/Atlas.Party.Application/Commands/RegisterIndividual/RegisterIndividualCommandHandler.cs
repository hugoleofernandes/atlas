using Atlas.Party.Application.Abstractions;
using Atlas.Party.Application.Repositories;
using Atlas.Party.Domain.Parties;
using Atlas.Party.Domain.Parties.Exceptions;
using Atlas.Party.Domain.Shared;
using Atlas.Party.Domain.Shared.Exceptions;
using Atlas.SharedKernel.Application;

namespace Atlas.Party.Application.Commands.RegisterIndividual;

public sealed class RegisterIndividualCommandHandler : IRegisterIndividualCommandHandler
{
    private readonly IIndividualRepository _individualRepository;
    private readonly IRequestContext _requestContext;
    private readonly IPartyUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public RegisterIndividualCommandHandler(
        IIndividualRepository individualRepository,
        IRequestContext requestContext,
        IPartyUnitOfWork uow
    )
    {
        _individualRepository = individualRepository;
        _requestContext = requestContext;
        _uow = uow;
    }

    public async Task<RegisterIndividualOutput> ExecuteAsync(RegisterIndividualCommand cmd, CancellationToken ct)
    {
        var tenantId = _requestContext.TenantId ?? throw new TenantContextNotResolvedException();

        var taxNumber = TaxNumber.Create(cmd.TaxNumber);

        if (await _individualRepository.ExistsWithTaxNumberAsync(tenantId, taxNumber.Value, ct))
            throw new DuplicateTaxNumberException(taxNumber.Value);

        var name = PersonName.Create(cmd.FirstName, cmd.LastName, cmd.MiddleName);
        var individual = Individual.Register(tenantId, taxNumber, name, cmd.BirthDate, cmd.Gender);
        individual.ReplaceAddresses(cmd.Addresses);

        await _individualRepository.AddAsync(individual, ct);

        return new RegisterIndividualOutput(individual.Id, individual.TaxNumber.Value, individual.Name.FullName);
    }
}
