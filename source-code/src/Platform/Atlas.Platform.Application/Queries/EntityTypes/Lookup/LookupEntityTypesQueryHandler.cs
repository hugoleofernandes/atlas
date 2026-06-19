namespace Atlas.Platform.Application.Queries.EntityTypes.Lookup;

public sealed class LookupEntityTypesQueryHandler(ILookupEntityTypesCache cache)
    : ILookupEntityTypesQueryHandler
{
    public async Task<IReadOnlyList<EntityTypeLookupDto>> ExecuteAsync(
        LookupEntityTypesQuery query,
        CancellationToken ct)
    {
        var items = await cache.GetAsync(ct);

        if (query.ModuleId is null)
            return items;

        return items
            .Where(x => x.ModuleId == query.ModuleId.Value)
            .ToList()
            .AsReadOnly();
    }
}
