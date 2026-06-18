using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Queries.Lookups;
using Atlas.Party.Application.Queries.Lookups.LookupClassificationTypes;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Lookups.LookupClassificationTypes;

public sealed class LookupClassificationTypesEndpoint(
    ILookupClassificationTypesQueryHandler handler,
    IPartyLookupLabelLocalizer lookupLabelLocalizer,
    IHandlerInvoker invoker
) : AtlasEndpoint<EmptyRequest, IReadOnlyList<LookupClassificationTypesResponse>>
{
    public override void Configure()
    {
        Get("bff/v1/party/lookups/classification-types");
        Policies($"permission:{PartyModulePermissions.Lookups.Read.Code}");
        Description(d => d.Produces<IReadOnlyList<LookupClassificationTypesResponse>>());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new LookupClassificationTypesQuery(), ct);
        var response = result.Map(x => LookupClassificationTypesResponse.FromList(x, lookupLabelLocalizer));
        await OkFromResultAsync(response, ct);
    }
}
