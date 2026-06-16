using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Commands.UpdateOrganization;
using Atlas.Party.BffApi.Endpoints.Shared;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Organizations.UpdateOrganization;

/// <summary>
/// Updates mutable company details of an existing organization. Does not change TaxNumber or TenantId.
/// </summary>
public sealed class UpdateOrganizationEndpoint(IUpdateOrganizationCommandHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<UpdateOrganizationRequest, UpdateOrganizationResponse>
{
    public override void Configure()
    {
        Put("bff/v1/party/organizations/{id}");
        Policies($"permission:{PartyModulePermissions.Organization.Update.Code}");
        Description(d => d.Produces<UpdateOrganizationResponse>());
    }

    public override async Task HandleAsync(UpdateOrganizationRequest req, CancellationToken ct)
    {
        var cmd = new UpdateOrganizationCommand(
            req.Id,
            req.LegalName,
            req.TradeName,
            req.LegalType,
            AddressRequestMapper.ToAddressInputs(req.Addresses)
        );
        var result = await invoker.InvokeAsync(handler, cmd, ct);
        await UpdatedFromResultAsync(result, UpdateOrganizationResponse.From, ct);
    }
}
