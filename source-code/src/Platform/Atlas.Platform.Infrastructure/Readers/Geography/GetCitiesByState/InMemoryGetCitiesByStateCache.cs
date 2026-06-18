using Atlas.Platform.Application.Queries.Geography.GetCitiesByState;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Platform.Infrastructure.Readers.Geography.GetCitiesByState;

public sealed class InMemoryGetCitiesByStateCache(IServiceScopeFactory scopeFactory) : IGetCitiesByStateCache
{
    private volatile Dictionary<string, Dictionary<string, IReadOnlyList<CityDto>>>? _cache;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<IReadOnlyList<CityDto>> GetAsync(string countryCode, string stateCode, CancellationToken ct)
    {
        await EnsureLoadedAsync(ct);
        return _cache!.TryGetValue(countryCode.ToUpperInvariant(), out var byState)
            && byState.TryGetValue(stateCode.ToUpperInvariant(), out var cities)
            ? cities
            : [];
    }

    public void Invalidate() => _cache = null;

    private async Task EnsureLoadedAsync(CancellationToken ct)
    {
        if (_cache is not null)
            return;

        await _lock.WaitAsync(ct);
        try
        {
            if (_cache is not null)
                return;

            using var scope = scopeFactory.CreateScope();
            var reader = scope.ServiceProvider.GetRequiredService<IGetCitiesByStateReader>();
            var allCities = await reader.ReadAsync(ct);

            _cache = allCities
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
