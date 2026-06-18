using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Party.Application.Queries.Lookups.LookupContactTypes;
using Atlas.Party.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Party.BffApi.Endpoints.Lookups.LookupContactTypes;

public sealed class LookupContactTypesEndpoint(ILookupContactTypesQueryHandler handler, IHandlerInvoker invoker)
    : AtlasEndpoint<EmptyRequest, IReadOnlyList<ContactTypeLookupDto>>
{
    public override void Configure()
    {
        Get("bff/v1/party/lookups/contact-types");
        Policies($"permission:{PartyModulePermissions.Lookups.Read.Code}");
        Description(d => d.Produces<IReadOnlyList<ContactTypeLookupDto>>());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new LookupContactTypesQuery(), ct);
        await OkFromResultAsync(result, ct);
    }
}
