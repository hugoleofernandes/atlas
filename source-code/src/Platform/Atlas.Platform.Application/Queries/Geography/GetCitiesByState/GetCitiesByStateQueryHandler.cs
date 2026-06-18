namespace Atlas.Platform.Application.Queries.Geography.GetCitiesByState;

public sealed class GetCitiesByStateQueryHandler(IGetCitiesByStateCache cache)
    : IGetCitiesByStateQueryHandler
{
    public Task<IReadOnlyList<CityDto>> ExecuteAsync(GetCitiesByStateQuery query, CancellationToken ct)
        => cache.GetAsync(query.CountryCode, query.StateCode, ct);
}
