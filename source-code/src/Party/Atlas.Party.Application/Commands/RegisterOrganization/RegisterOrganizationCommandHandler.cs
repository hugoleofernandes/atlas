using Atlas.Party.Application.Abstractions;
using Atlas.Party.Application.Repositories;
using Atlas.Party.Domain.Parties;
using Atlas.Party.Domain.Parties.Exceptions;
using Atlas.Party.Domain.Shared;
using Atlas.Party.Domain.Shared.Exceptions;
using Atlas.SharedKernel.Application;

namespace Atlas.Party.Application.Commands.RegisterOrganization;

public sealed class RegisterOrganizationCommandHandler : IRegisterOrganizationCommandHandler
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IRequestContext _requestContext;
    private readonly IPartyUnitOfWork _uow;

    public IUnitOfWork UnitOfWork => _uow;

    public RegisterOrganizationCommandHandler(
        IOrganizationRepository organizationRepository,
        IRequestContext requestContext,
        IPartyUnitOfWork uow
    )
    {
        _organizationRepository = organizationRepository;
        _requestContext = requestContext;
        _uow = uow;
    }

    public async Task<RegisterOrganizationOutput> ExecuteAsync(RegisterOrganizationCommand cmd, CancellationToken ct)
    {
        var tenantId = _requestContext.TenantId ?? throw new TenantContextNotResolvedException();

        var taxNumber = TaxNumber.Create(cmd.TaxNumber);

        if (await _organizationRepository.ExistsWithTaxNumberAsync(tenantId, taxNumber.Value, ct))
            throw new DuplicateTaxNumberException(taxNumber.Value);

        var organization = Organization.Register(tenantId, taxNumber, cmd.LegalName, cmd.TradeName, cmd.LegalType);
        organization.ReplaceAddresses(cmd.Addresses);

        await _organizationRepository.AddAsync(organization, ct);

        return new RegisterOrganizationOutput(organization.Id, organization.TaxNumber.Value, organization.LegalName);
    }
}
