using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Commands.DeactivateIndividual;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Individuals.DeactivateIndividual;

/// <summary>
/// Deactivates an individual. Downstream consumers are notified via PartyDeactivatedDomainEvent.
/// </summary>
public sealed class DeactivateIndividualEndpoint(IDeactivateIndividualCommandHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<DeactivateIndividualRequest, EmptyResponse>
{
    public override void Configure()
    {
        Patch("bff/v1/party/individuals/{id}/deactivate");
        Policies($"permission:{PartyModulePermissions.Individual.Deactivate.Code}");
        Description(d => d.Produces(204));
    }

    public override async Task HandleAsync(DeactivateIndividualRequest req, CancellationToken ct)
    {
        var cmd = new DeactivateIndividualCommand(req.Id);
        var result = await invoker.InvokeAsync(handler, cmd, ct);
        await UpdatedNoContentFromResultAsync(result, ct);
    }
}
