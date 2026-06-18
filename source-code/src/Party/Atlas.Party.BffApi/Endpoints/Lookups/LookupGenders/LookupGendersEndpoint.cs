using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Queries.Lookups.LookupGenders;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Lookups.LookupGenders;

public sealed class LookupGendersEndpoint(ILookupGendersQueryHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<EmptyRequest, IReadOnlyList<GenderLookupDto>>
{
    public override void Configure()
    {
        Get("bff/v1/party/lookups/genders");
        Policies($"permission:{PartyModulePermissions.Lookups.Read.Code}");
        Description(d => d.Produces<IReadOnlyList<GenderLookupDto>>());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new LookupGendersQuery(), ct);
        await OkFromResultAsync(result, ct);
    }
}
