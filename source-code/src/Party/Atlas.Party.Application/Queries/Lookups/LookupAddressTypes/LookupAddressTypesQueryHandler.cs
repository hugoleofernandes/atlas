namespace Atlas.Party.Application.Queries.Lookups.LookupAddressTypes;

public sealed class LookupAddressTypesQueryHandler(ILookupAddressTypesReader reader)
    : ILookupAddressTypesQueryHandler
{
    public Task<IReadOnlyList<AddressTypeLookupDto>> ExecuteAsync(LookupAddressTypesQuery query, CancellationToken ct)
        => reader.LookupAsync(ct);
}
