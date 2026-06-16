using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Commands.DeactivateOrganization;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Organizations.DeactivateOrganization;

/// <summary>
/// Deactivates an organization. Downstream consumers are notified via PartyDeactivatedDomainEvent.
/// </summary>
public sealed class DeactivateOrganizationEndpoint(IDeactivateOrganizationCommandHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<DeactivateOrganizationRequest, EmptyResponse>
{
    public override void Configure()
    {
        Patch("bff/v1/party/organizations/{id}/deactivate");
        Policies($"permission:{PartyModulePermissions.Organization.Deactivate.Code}");
        Description(d => d.Produces(204));
    }

    public override async Task HandleAsync(DeactivateOrganizationRequest req, CancellationToken ct)
    {
        var cmd = new DeactivateOrganizationCommand(req.Id);
        var result = await invoker.InvokeAsync(handler, cmd, ct);
        await UpdatedNoContentFromResultAsync(result, ct);
    }
}
