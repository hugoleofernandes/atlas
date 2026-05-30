namespace Atlas.Platform.Application.Queries.EntityTypes.Lookup;

public interface ILookupEntityTypesReader
{
    Task<IReadOnlyList<EntityTypeLookupDto>> LookupAsync(CancellationToken ct);
}
