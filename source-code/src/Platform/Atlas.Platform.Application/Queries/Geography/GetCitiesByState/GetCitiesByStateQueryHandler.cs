using Atlas.Platform.Application.Queries.Geography;

namespace Atlas.Platform.Application.Queries.Geography.GetCitiesByState;

public sealed class GetCitiesByStateQueryHandler(IGeographyCache cache)
    : IGetCitiesByStateQueryHandler
{
    public Task<IReadOnlyList<CityDto>> ExecuteAsync(GetCitiesByStateQuery query, CancellationToken ct)
        => cache.GetCitiesByStateCodeAsync(query.CountryCode, query.StateCode, ct);
}
