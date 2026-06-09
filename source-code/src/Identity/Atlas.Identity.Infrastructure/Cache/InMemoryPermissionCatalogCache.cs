using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Infrastructure.Readers.Permissions;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Identity.Infrastructure.Cache;

/// <summary>
/// Singleton in-memory cache for the permission catalog.
/// Thread-safe: uses a semaphore to prevent multiple concurrent DB loads on cache miss.
/// No TTL — invalidate explicitly at every write point (seeder, future admin actions).
///
/// Resolve PermissionCatalogReader pelo tipo concreto para não registar
/// IPermissionCatalogReader no DI público. Ver IPermissionCatalogReader.
/// </summary>
public sealed class InMemoryPermissionCatalogCache(IServiceScopeFactory scopeFactory) : IPermissionCatalogCache
{
    private volatile IReadOnlyList<PermissionRecord>? _cache;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<IReadOnlyList<PermissionRecord>> GetAllActiveAsync(CancellationToken ct)
    {
        if (_cache is not null)
            return _cache;

        await _lock.WaitAsync(ct);
        try
        {
            if (_cache is not null) // double-check após espera no lock
                return _cache;

            using var scope = scopeFactory.CreateScope();
            var reader = scope.ServiceProvider.GetRequiredService<PermissionCatalogReader>();
            _cache = await reader.GetAllActiveAsync(ct);
            return _cache;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Invalidate() => _cache = null;
}
