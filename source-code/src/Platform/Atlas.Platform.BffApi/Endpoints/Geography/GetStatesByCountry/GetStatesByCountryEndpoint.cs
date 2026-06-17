using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Platform.Application.Queries.Geography.GetStatesByCountry;
using Atlas.Platform.Contracts.Permissions;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.AspNetCore.Http;

namespace Atlas.Platform.BffApi.Endpoints.Geography.GetStatesByCountry;

/// <summary>
/// Returns all active states for a given country code.
/// Results are served from the in-memory geography cache — no DB round-trip per request.
/// </summary>
public sealed class GetStatesByCountryEndpoint(
    IGetStatesByCountryQueryHandler handler,
    IHandlerInvoker invoker)
    : AtlasEndpoint<GetStatesByCountryRequest, IReadOnlyList<GetStatesByCountryResponse>>
{
    public override void Configure()
    {
        Get("bff/v1/platform/countries/{CountryCode}/states");
        Policies($"permission:{PlatformModulePermissions.Geography.Read}");
        Description(d => d.Produces<IReadOnlyList<GetStatesByCountryResponse>>());
    }

    public override async Task HandleAsync(GetStatesByCountryRequest req, CancellationToken ct)
    {
        var query  = new GetStatesByCountryQuery(req.CountryCode);
        var result = await invoker.InvokeAsync(handler, query, ct);
        await OkFromResultAsync(
            result.Map(states => states.Select(GetStatesByCountryResponse.From).ToList() as IReadOnlyList<GetStatesByCountryResponse>),
            ct);
    }
}
