using Atlas.Platform.Application.Queries.Geography.GetStatesByCountry;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Platform.Infrastructure.Readers.Geography.GetStatesByCountry;

public sealed class InMemoryGetStatesByCountryCache(IServiceScopeFactory scopeFactory) : IGetStatesByCountryCache
{
    private volatile Dictionary<string, IReadOnlyList<StateDto>>? _cache;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<IReadOnlyList<StateDto>> GetAsync(string countryCode, CancellationToken ct)
    {
        await EnsureLoadedAsync(ct);
        return _cache!.TryGetValue(countryCode.ToUpperInvariant(), out var states) ? states : [];
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
            var reader = scope.ServiceProvider.GetRequiredService<IGetStatesByCountryReader>();
            var allStates = await reader.ReadAsync(ct);

            _cache = allStates
                .GroupBy(s => s.CountryCode)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<StateDto>)g.ToList());
        }
        finally
        {
            _lock.Release();
        }
    }
}
