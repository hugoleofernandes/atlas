using Atlas.Platform.Application.Queries.Geography.GetCitiesByState;
using Atlas.Platform.Application.Queries.Geography.GetStatesByCountry;

namespace Atlas.Platform.Application.Queries.Geography;

/// <summary>
/// Singleton in-memory cache for geography reference data (states and cities).
/// Loaded from DB on first access; no TTL — data is seeded once and never changes at runtime.
/// </summary>
public interface IGeographyCache
{
    Task<IReadOnlyList<StateDto>> GetStatesByCountryCodeAsync(string countryCode, CancellationToken ct);
    Task<IReadOnlyList<CityDto>> GetCitiesByStateCodeAsync(string countryCode, string stateCode, CancellationToken ct);
    void Invalidate();
}
