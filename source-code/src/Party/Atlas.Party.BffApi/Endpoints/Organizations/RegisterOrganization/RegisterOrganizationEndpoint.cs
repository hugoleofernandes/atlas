using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Commands.RegisterOrganization;
using Atlas.Party.BffApi.Endpoints.Shared;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Organizations.RegisterOrganization;

/// <summary>
/// Registers a new organization (legal entity) for the authenticated user's tenant.
/// </summary>
public sealed class RegisterOrganizationEndpoint(IRegisterOrganizationCommandHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<RegisterOrganizationRequest, RegisterOrganizationResponse>
{
    public override void Configure()
    {
        Post("bff/v1/party/organizations");
        Policies($"permission:{PartyModulePermissions.Organization.Create.Code}");
        Description(d => d.Produces<RegisterOrganizationResponse>(201));
    }

    public override async Task HandleAsync(RegisterOrganizationRequest req, CancellationToken ct)
    {
        var cmd = new RegisterOrganizationCommand(
            req.TaxNumber,
            req.LegalName,
            req.TradeName,
            req.LegalType,
            AddressRequestMapper.ToAddressInputs(req.Addresses)
        );
        var result = await invoker.InvokeAsync(handler, cmd, ct);
        await CreatedFromResultAsync(result, RegisterOrganizationResponse.From, ct);
    }
}
