using Atlas.Platform.Application.Queries.Geography;

namespace Atlas.Platform.Application.Queries.Geography.GetStatesByCountry;

public sealed class GetStatesByCountryQueryHandler(IGeographyCache cache)
    : IGetStatesByCountryQueryHandler
{
    public Task<IReadOnlyList<StateDto>> ExecuteAsync(GetStatesByCountryQuery query, CancellationToken ct)
        => cache.GetStatesByCountryCodeAsync(query.CountryCode, ct);
}
