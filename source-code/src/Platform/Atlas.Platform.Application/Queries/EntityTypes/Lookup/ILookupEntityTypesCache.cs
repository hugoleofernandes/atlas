namespace Atlas.Platform.Application.Queries.EntityTypes.Lookup;

public interface ILookupEntityTypesCache
{
    Task<IReadOnlyList<EntityTypeLookupDto>> GetAsync(CancellationToken ct);
    void Invalidate();
}
