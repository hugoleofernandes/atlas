namespace Atlas.BuildingBlocks.Permissions;

/// <summary>
/// In-memory cache for the permission catalog.
/// Singleton — survives across requests within the same process.
/// No TTL — permissions change rarely and only at known write points.
/// Invalidate explicitly whenever the catalog is written (seeder, future admin endpoints).
/// </summary>
public interface IPermissionCatalogCache
{
    /// <summary>Returns all permissions, active and inactive, loading from DB on first call or after invalidation.</summary>
    Task<IReadOnlyList<PermissionRecord>> GetAllAsync(CancellationToken ct);

    /// <summary>Returns all active permissions, loading from DB on first call or after invalidation.</summary>
    Task<IReadOnlyList<PermissionRecord>> GetAllActiveAsync(CancellationToken ct);

    /// <summary>Clears the cache. Next call to GetAllAsync/GetAllActiveAsync reloads from DB.</summary>
    void Invalidate();
}
