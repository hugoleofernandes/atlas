using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Platform.Application.Queries.Geography.GetCitiesByState;
using Atlas.Platform.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Platform.BffApi.Endpoints.Geography.GetCitiesByState;

/// <summary>
/// Returns all active cities for a given country code and state code.
/// Results are served from the in-memory geography cache — no DB round-trip per request.
/// </summary>
public sealed class GetCitiesByStateEndpoint(
    IGetCitiesByStateQueryHandler handler,
    IHandlerInvoker invoker)
    : AtlasEndpoint<GetCitiesByStateRequest, IReadOnlyList<GetCitiesByStateResponse>>
{
    public override void Configure()
    {
        Get("bff/v1/platform/countries/{CountryCode}/states/{StateCode}/cities");
        Policies($"permission:{PlatformModulePermissions.Geography.Read}");
        Description(d => d.Produces<IReadOnlyList<GetCitiesByStateResponse>>());
    }

    public override async Task HandleAsync(GetCitiesByStateRequest req, CancellationToken ct)
    {
        var query  = new GetCitiesByStateQuery(req.CountryCode, req.StateCode);
        var result = await invoker.InvokeAsync(handler, query, ct);
        await OkFromResultAsync(
            result.Map(cities => cities.Select(GetCitiesByStateResponse.From).ToList() as IReadOnlyList<GetCitiesByStateResponse>),
            ct);
    }
}
