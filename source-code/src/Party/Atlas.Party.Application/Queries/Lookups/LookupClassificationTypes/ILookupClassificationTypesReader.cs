namespace Atlas.Party.Application.Queries.Lookups.LookupClassificationTypes;

public interface ILookupClassificationTypesReader
{
    Task<IReadOnlyList<ClassificationTypeLookupDto>> LookupAsync(CancellationToken ct);
}
