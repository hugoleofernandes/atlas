using Atlas.Party.Application.Abstractions;
using Atlas.Party.Application.Repositories;
using Atlas.Party.Domain.Parties.Exceptions;
using Atlas.SharedKernel.Application;

namespace Atlas.Party.Application.Commands.UpdateOrganization;

public sealed class UpdateOrganizationCommandHandler : IUpdateOrganizationCommandHandler
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IPartyUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public UpdateOrganizationCommandHandler(IOrganizationRepository organizationRepository, IPartyUnitOfWork uow)
    {
        _organizationRepository = organizationRepository;
        _uow = uow;
    }

    public async Task<UpdateOrganizationOutput> ExecuteAsync(UpdateOrganizationCommand cmd, CancellationToken ct)
    {
        var organization =
            await _organizationRepository.GetByIdAsync(cmd.PartyId, ct)
            ?? throw new OrganizationNotFoundException(cmd.PartyId);

        organization.Update(cmd.LegalName, cmd.TradeName, cmd.LegalType);
        organization.ReplaceAddresses(cmd.Addresses);

        return new UpdateOrganizationOutput(organization.Id, organization.LegalName);
    }
}
