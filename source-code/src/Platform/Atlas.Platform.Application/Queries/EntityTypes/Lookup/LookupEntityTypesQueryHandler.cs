namespace Atlas.Platform.Application.Queries.EntityTypes.Lookup;

public sealed class LookupEntityTypesQueryHandler(ILookupEntityTypesCache cache)
    : ILookupEntityTypesQueryHandler
{
    public Task<IReadOnlyList<EntityTypeLookupDto>> ExecuteAsync(
        LookupEntityTypesQuery query,
        CancellationToken ct)
        => cache.GetAsync(ct);
}
