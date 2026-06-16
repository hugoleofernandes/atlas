using Atlas.Party.Application.Abstractions;
using Atlas.Party.Application.Repositories;
using Atlas.Party.Domain.Parties.Exceptions;
using Atlas.SharedKernel.Application;

namespace Atlas.Party.Application.Commands.DeactivateIndividual;

public sealed class DeactivateIndividualCommandHandler : IDeactivateIndividualCommandHandler
{
    private readonly IIndividualRepository _individualRepository;
    private readonly IPartyUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public DeactivateIndividualCommandHandler(IIndividualRepository individualRepository, IPartyUnitOfWork uow)
    {
        _individualRepository = individualRepository;
        _uow = uow;
    }

    public async Task<DeactivateIndividualOutput> ExecuteAsync(DeactivateIndividualCommand cmd, CancellationToken ct)
    {
        var individual =
            await _individualRepository.GetByIdAsync(cmd.PartyId, ct)
            ?? throw new IndividualNotFoundException(cmd.PartyId);

        individual.Deactivate();

        return new DeactivateIndividualOutput(individual.Id, individual.IsActive);
    }
}
