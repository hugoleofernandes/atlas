namespace Atlas.Party.Application.Queries.Lookups.LookupGenders;

public interface ILookupGendersReader
{
    Task<IReadOnlyList<GenderLookupDto>> LookupAsync(CancellationToken ct);
}
