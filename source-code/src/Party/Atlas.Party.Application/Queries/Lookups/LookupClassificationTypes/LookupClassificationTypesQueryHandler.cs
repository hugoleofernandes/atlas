namespace Atlas.Party.Application.Queries.Lookups.LookupClassificationTypes;

public sealed class LookupClassificationTypesQueryHandler(ILookupClassificationTypesReader reader)
    : ILookupClassificationTypesQueryHandler
{
    public Task<IReadOnlyList<ClassificationTypeLookupDto>> ExecuteAsync(
        LookupClassificationTypesQuery query,
        CancellationToken ct)
        => reader.LookupAsync(ct);
}
