using Atlas.Platform.Application.Queries.Geography.GetCitiesByState;
using Atlas.Platform.Application.Queries.Geography.GetStatesByCountry;

namespace Atlas.Platform.Application.Queries.Geography;

/// <summary>
/// Dapper reader that bulk-loads all geography reference data for the in-memory cache.
/// Used exclusively by IGeographyCache implementations — not called directly by handlers.
/// </summary>
public interface IGeographyReader
{
    Task<IReadOnlyList<StateDto>> LoadAllStatesAsync(CancellationToken ct);
    Task<IReadOnlyList<CityDto>> LoadAllCitiesAsync(CancellationToken ct);
}
