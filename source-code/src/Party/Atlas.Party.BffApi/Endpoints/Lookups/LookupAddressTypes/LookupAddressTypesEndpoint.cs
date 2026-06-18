using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Queries.Lookups;
using Atlas.Party.Application.Queries.Lookups.LookupAddressTypes;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Lookups.LookupAddressTypes;

public sealed class LookupAddressTypesEndpoint(
    ILookupAddressTypesQueryHandler handler,
    IPartyLookupLabelLocalizer lookupLabelLocalizer,
    IHandlerInvoker invoker
) : AtlasEndpoint<EmptyRequest, IReadOnlyList<LookupAddressTypesResponse>>
{
    public override void Configure()
    {
        Get("bff/v1/party/lookups/address-types");
        Policies($"permission:{PartyModulePermissions.Lookups.Read.Code}");
        Description(d => d.Produces<IReadOnlyList<LookupAddressTypesResponse>>());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new LookupAddressTypesQuery(), ct);
        var response = result.Map(x => LookupAddressTypesResponse.FromList(x, lookupLabelLocalizer));
        await OkFromResultAsync(response, ct);
    }
}
