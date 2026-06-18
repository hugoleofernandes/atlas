using Atlas.Party.Application.Queries.Lookups;
using Atlas.Party.Application.Queries.Lookups.LookupContactTypes;

namespace Atlas.Party.BffApi.Endpoints.Lookups.LookupContactTypes;

public sealed record LookupContactTypesResponse(string Code, string Name)
{
    public static IReadOnlyList<LookupContactTypesResponse> FromList(
        IReadOnlyList<ContactTypeLookupDto> result,
        IPartyLookupLabelLocalizer localizer
    )
    {
        return result
            .Select(x => new LookupContactTypesResponse(
                Code: x.Code,
                Name: localizer.GetContactTypeName(x.Code)
            ))
            .OrderBy(x => x.Name)
            .ToList();
    }
}
