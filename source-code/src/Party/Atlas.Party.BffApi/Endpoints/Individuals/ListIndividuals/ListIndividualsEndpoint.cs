using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Queries.Individuals;
using Atlas.Party.Application.Queries.Individuals.ListIndividuals;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Individuals.ListIndividuals;

public sealed class ListIndividualsEndpoint(IListIndividualsQueryHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<ListIndividualsRequest, IReadOnlyList<IndividualDto>>
{
    public override void Configure()
    {
        Get("bff/v1/party/individuals");
        Policies($"permission:{PartyModulePermissions.Individual.Read.Code}");
        Description(d => d.Produces<IReadOnlyList<IndividualDto>>());
    }

    public override async Task HandleAsync(ListIndividualsRequest req, CancellationToken ct)
    {
        var query = new ListIndividualsQuery(req.IsActive);
        var result = await invoker.InvokeAsync(handler, query, ct);
        await OkFromResultAsync(result, ct);
    }
}
