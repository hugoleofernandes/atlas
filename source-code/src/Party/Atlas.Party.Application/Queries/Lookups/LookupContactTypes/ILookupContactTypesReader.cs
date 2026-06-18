namespace Atlas.Party.Application.Queries.Lookups.LookupContactTypes;

public interface ILookupContactTypesReader
{
    Task<IReadOnlyList<ContactTypeLookupDto>> LookupAsync(CancellationToken ct);
}
