namespace Atlas.Platform.Application.Queries.EntityTypes.Lookup;

public sealed class LookupEntityTypesQueryHandler : ILookupEntityTypesQueryHandler
{
    private readonly ILookupEntityTypesReader _reader;

    public LookupEntityTypesQueryHandler(ILookupEntityTypesReader reader)
    {
        _reader = reader;
    }

    public Task<IReadOnlyList<EntityTypeLookupDto>> ExecuteAsync(
        LookupEntityTypesQuery query,
        CancellationToken ct)
        => _reader.LookupAsync(ct);
}
