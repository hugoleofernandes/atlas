using Atlas.Platform.Application.Queries.EntityTypes.Lookup;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Platform.Infrastructure.Readers.EntityTypes.Lookup;

public sealed class InMemoryLookupEntityTypesCache(IServiceScopeFactory scopeFactory)
    : ILookupEntityTypesCache
{
    private volatile IReadOnlyList<EntityTypeLookupDto>? _cache;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<IReadOnlyList<EntityTypeLookupDto>> GetAsync(CancellationToken ct)
    {
        if (_cache is not null)
            return _cache;

        await _lock.WaitAsync(ct);
        try
        {
            if (_cache is not null)
                return _cache;

            using var scope = scopeFactory.CreateScope();
            var reader = scope.ServiceProvider.GetRequiredService<ILookupEntityTypesReader>();
            _cache = await reader.LookupAsync(ct);
            return _cache;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Invalidate() => _cache = null;
}
