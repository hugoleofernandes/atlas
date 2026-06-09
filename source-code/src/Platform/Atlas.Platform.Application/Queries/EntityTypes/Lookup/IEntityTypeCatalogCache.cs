namespace Atlas.Platform.Application.Queries.EntityTypes.Lookup;

/// <summary>
/// In-memory cache for the entity type catalog.
/// Singleton — survives across requests within the same process.
/// No TTL — entity types change only at known write points (seeder, future admin actions).
/// Invalidate explicitly whenever the catalog is written.
/// </summary>
public interface IEntityTypeCatalogCache
{
    /// <summary>Returns all active entity types, loading from DB on first call or after invalidation.</summary>
    Task<IReadOnlyList<EntityTypeLookupDto>> GetAllActiveAsync(CancellationToken ct);

    /// <summary>Clears the cache. Next call to GetAllActiveAsync reloads from DB.</summary>
    void Invalidate();
}
