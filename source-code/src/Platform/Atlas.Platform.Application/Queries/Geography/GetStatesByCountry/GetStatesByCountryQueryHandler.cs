namespace Atlas.Platform.Application.Queries.Geography.GetStatesByCountry;

public sealed class GetStatesByCountryQueryHandler(IGetStatesByCountryCache cache)
    : IGetStatesByCountryQueryHandler
{
    public Task<IReadOnlyList<StateDto>> ExecuteAsync(GetStatesByCountryQuery query, CancellationToken ct)
        => cache.GetAsync(query.CountryCode, ct);
}
