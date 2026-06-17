using Atlas.Platform.Application.Queries.Geography;
using Atlas.Platform.Application.Queries.Geography.GetCitiesByState;
using Atlas.Platform.Application.Queries.Geography.GetStatesByCountry;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Platform.Infrastructure.Readers.Geography;

/// <summary>
/// Singleton in-memory cache for geography reference data.
/// Thread-safe: uses a semaphore to prevent concurrent DB loads on cache miss.
/// No TTL — states and cities are seeded once and never change at runtime.
/// </summary>
public sealed class InMemoryGeographyCache(IServiceScopeFactory scopeFactory) : IGeographyCache
{
    private volatile Dictionary<string, IReadOnlyList<StateDto>>? _states;
    private volatile Dictionary<string, Dictionary<string, IReadOnlyList<CityDto>>>? _cities;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<IReadOnlyList<StateDto>> GetStatesByCountryCodeAsync(string countryCode, CancellationToken ct)
    {
        await EnsureLoadedAsync(ct);
        return _states!.TryGetValue(countryCode.ToUpperInvariant(), out var states) ? states : [];
    }

    public async Task<IReadOnlyList<CityDto>> GetCitiesByStateCodeAsync(string countryCode, string stateCode, CancellationToken ct)
    {
        await EnsureLoadedAsync(ct);
        return _cities!.TryGetValue(countryCode.ToUpperInvariant(), out var byState)
            && byState.TryGetValue(stateCode.ToUpperInvariant(), out var cities)
            ? cities
            : [];
    }

    public void Invalidate()
    {
        _states = null;
        _cities = null;
    }

    private async Task EnsureLoadedAsync(CancellationToken ct)
    {
        if (_states is not null)
            return;

        await _lock.WaitAsync(ct);
        try
        {
            if (_states is not null) // double-check after acquiring lock
                return;

            using var scope = scopeFactory.CreateScope();
            var reader = scope.ServiceProvider.GetRequiredService<IGeographyReader>();

            var allStates = await reader.LoadAllStatesAsync(ct);
            var allCities = await reader.LoadAllCitiesAsync(ct);

            _states = allStates
                .GroupBy(s => s.CountryCode)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<StateDto>)g.ToList());

            _cities = allCities
                .GroupBy(c => c.CountryCode)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(c => c.StateCode)
                          .ToDictionary(sg => sg.Key, sg => (IReadOnlyList<CityDto>)sg.ToList()));
        }
        finally
        {
            _lock.Release();
        }
    }
}
