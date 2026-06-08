namespace Atlas.Platform.Application.Queries.EntityTypes.Lookup;

public sealed class LookupEntityTypesQueryHandler(IEntityTypeCatalogCache cache)
    : ILookupEntityTypesQueryHandler
{
    public Task<IReadOnlyList<EntityTypeLookupDto>> ExecuteAsync(
        LookupEntityTypesQuery query,
        CancellationToken ct)
        => cache.GetAllActiveAsync(ct);
}
