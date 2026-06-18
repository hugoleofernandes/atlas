using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Queries.Lookups.LookupAddressTypes;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Lookups.LookupAddressTypes;

public sealed class LookupAddressTypesEndpoint(ILookupAddressTypesQueryHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<EmptyRequest, IReadOnlyList<AddressTypeLookupDto>>
{
    public override void Configure()
    {
        Get("bff/v1/party/lookups/address-types");
        Policies($"permission:{PartyModulePermissions.Lookups.Read.Code}");
        Description(d => d.Produces<IReadOnlyList<AddressTypeLookupDto>>());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new LookupAddressTypesQuery(), ct);
        await OkFromResultAsync(result, ct);
    }
}
