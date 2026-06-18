using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Queries.Lookups;
using Atlas.Party.Application.Queries.Persons.ListPersons;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Persons.ListPersons;

public sealed class ListPersonsEndpoint(
    IListPersonsQueryHandler handler,
    IPartyLookupLabelLocalizer lookupLabelLocalizer,
    IHandlerInvoker invoker
) : AtlasEndpoint<ListPersonsRequest, IReadOnlyList<ListPersonsResponse>>
{
    public override void Configure()
    {
        Get("bff/v1/party/persons");
        Policies($"permission:{PartyModulePermissions.Person.Read.Code}");
        Description(d => d.Produces<IReadOnlyList<ListPersonsResponse>>());
    }

    public override async Task HandleAsync(ListPersonsRequest req, CancellationToken ct)
    {
        var query = new ListPersonsQuery(req.IsActive, req.Classification);
        var result = await invoker.InvokeAsync(handler, query, ct);
        var response = result.Map(x => ListPersonsResponse.FromList(x, lookupLabelLocalizer));
        await OkFromResultAsync(response, ct);
    }
}
