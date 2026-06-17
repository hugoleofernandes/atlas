using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Commands.DeactivatePerson;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Persons.DeactivatePerson;

/// <summary>
/// Deactivates a person. Downstream consumers are notified via PartyDeactivatedDomainEvent.
/// </summary>
public sealed class DeactivatePersonEndpoint(IDeactivatePersonCommandHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<DeactivatePersonRequest, EmptyResponse>
{
    public override void Configure()
    {
        Patch("bff/v1/party/persons/{id}/deactivate");
        Policies($"permission:{PartyModulePermissions.Person.Deactivate.Code}");
        Description(d => d.Produces(204));
    }

    public override async Task HandleAsync(DeactivatePersonRequest req, CancellationToken ct)
    {
        var cmd = new DeactivatePersonCommand(req.Id);
        var result = await invoker.InvokeAsync(handler, cmd, ct);
        await UpdatedNoContentFromResultAsync(result, ct);
    }
}

