using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Platform.Application.Queries.Lookups;
using Atlas.Platform.Application.Queries.Lookups.LookupStatuses;
using Atlas.Platform.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Atlas.Platform.BffApi.Endpoints.Lookups.LookupStatuses;

public sealed class LookupStatusesEndpoint(
    ILookupStatusesQueryHandler handler,
    IPlatformLookupLabelLocalizer lookupLabelLocalizer,
    IHandlerInvoker invoker
) : AtlasEndpoint<EmptyRequest, IReadOnlyList<LookupStatusesResponse>>
{
    public override void Configure()
    {
        Get("bff/v1/platform/lookups/statuses");
        Policies($"permission:{PlatformModulePermissions.Lookups.Read}");
        Description(d => d.Produces<IReadOnlyList<LookupStatusesResponse>>());
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var result = await invoker.InvokeAsync(handler, new LookupStatusesQuery(), ct);
        var response = result.Map(x => LookupStatusesResponse.FromList(x, lookupLabelLocalizer));
        await OkFromResultAsync(response, ct);
    }
}
