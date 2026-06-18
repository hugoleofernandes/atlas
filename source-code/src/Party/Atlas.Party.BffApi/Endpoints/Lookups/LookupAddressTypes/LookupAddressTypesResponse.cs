using Atlas.Party.Application.Queries.Lookups;
using Atlas.Party.Application.Queries.Lookups.LookupAddressTypes;

namespace Atlas.Party.BffApi.Endpoints.Lookups.LookupAddressTypes;

public sealed record LookupAddressTypesResponse(string Code, string Name)
{
    public static IReadOnlyList<LookupAddressTypesResponse> FromList(
        IReadOnlyList<AddressTypeLookupDto> result,
        IPartyLookupLabelLocalizer localizer
    )
    {
        return result
            .Select(x => new LookupAddressTypesResponse(
                Code: x.Code,
                Name: localizer.GetAddressTypeName(x.Code)
            ))
            .OrderBy(x => x.Name)
            .ToList();
    }
}
