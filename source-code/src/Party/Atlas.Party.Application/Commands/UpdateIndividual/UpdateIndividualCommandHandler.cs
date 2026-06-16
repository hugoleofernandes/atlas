using Atlas.Party.Application.Abstractions;
using Atlas.Party.Application.Repositories;
using Atlas.Party.Domain.Parties.Exceptions;
using Atlas.Party.Domain.Shared;
using Atlas.SharedKernel.Application;

namespace Atlas.Party.Application.Commands.UpdateIndividual;

public sealed class UpdateIndividualCommandHandler : IUpdateIndividualCommandHandler
{
    private readonly IIndividualRepository _individualRepository;
    private readonly IPartyUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public UpdateIndividualCommandHandler(IIndividualRepository individualRepository, IPartyUnitOfWork uow)
    {
        _individualRepository = individualRepository;
        _uow = uow;
    }

    public async Task<UpdateIndividualOutput> ExecuteAsync(UpdateIndividualCommand cmd, CancellationToken ct)
    {
        var individual =
            await _individualRepository.GetByIdAsync(cmd.PartyId, ct)
            ?? throw new IndividualNotFoundException(cmd.PartyId);

        var name = PersonName.Create(cmd.FirstName, cmd.LastName, cmd.MiddleName);
        individual.Update(name, cmd.BirthDate, cmd.Gender);
        individual.ReplaceAddresses(cmd.Addresses);

        return new UpdateIndividualOutput(individual.Id, individual.Name.FullName);
    }
}
