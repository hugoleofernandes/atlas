using Atlas.Party.Application.Abstractions;
using Atlas.Party.Application.Repositories;
using Atlas.Party.Domain.Parties.Exceptions;
using Atlas.SharedKernel.Application;

namespace Atlas.Party.Application.Commands.DeactivateOrganization;

public sealed class DeactivateOrganizationCommandHandler : IDeactivateOrganizationCommandHandler
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IPartyUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public DeactivateOrganizationCommandHandler(IOrganizationRepository organizationRepository, IPartyUnitOfWork uow)
    {
        _organizationRepository = organizationRepository;
        _uow = uow;
    }

    public async Task<DeactivateOrganizationOutput> ExecuteAsync(DeactivateOrganizationCommand cmd, CancellationToken ct)
    {
        var organization =
            await _organizationRepository.GetByIdAsync(cmd.PartyId, ct)
            ?? throw new OrganizationNotFoundException(cmd.PartyId);

        organization.Deactivate();

        return new DeactivateOrganizationOutput(organization.Id, organization.IsActive);
    }
}
