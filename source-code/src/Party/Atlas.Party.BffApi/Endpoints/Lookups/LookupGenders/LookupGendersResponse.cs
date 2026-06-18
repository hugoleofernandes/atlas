using Atlas.Party.Application.Queries.Lookups;
using Atlas.Party.Application.Queries.Lookups.LookupGenders;

namespace Atlas.Party.BffApi.Endpoints.Lookups.LookupGenders;

public sealed record LookupGendersResponse(string Code, string Name)
{
    public static IReadOnlyList<LookupGendersResponse> FromList(
        IReadOnlyList<GenderLookupDto> result,
        IPartyLookupLabelLocalizer localizer
    )
    {
        return result
            .Select(x => new LookupGendersResponse(
                Code: x.Code,
                Name: localizer.GetGenderName(x.Code)
            ))
            .OrderBy(x => x.Name)
            .ToList();
    }
}
