namespace Atlas.Party.Application.Queries.Lookups.LookupGenders;

public sealed class LookupGendersQueryHandler(ILookupGendersReader reader)
    : ILookupGendersQueryHandler
{
    public Task<IReadOnlyList<GenderLookupDto>> ExecuteAsync(LookupGendersQuery query, CancellationToken ct)
        => reader.LookupAsync(ct);
}
