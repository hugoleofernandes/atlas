namespace Atlas.Party.Application.Queries.Lookups.LookupAddressTypes;

public interface ILookupAddressTypesReader
{
    Task<IReadOnlyList<AddressTypeLookupDto>> LookupAsync(CancellationToken ct);
}
