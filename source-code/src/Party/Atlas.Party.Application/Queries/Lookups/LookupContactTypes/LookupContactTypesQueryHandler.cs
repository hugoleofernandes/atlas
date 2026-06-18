namespace Atlas.Party.Application.Queries.Lookups.LookupContactTypes;

public sealed class LookupContactTypesQueryHandler(ILookupContactTypesReader reader)
    : ILookupContactTypesQueryHandler
{
    public Task<IReadOnlyList<ContactTypeLookupDto>> ExecuteAsync(LookupContactTypesQuery query, CancellationToken ct)
        => reader.LookupAsync(ct);
}
