using Atlas.Party.Application.Queries.Lookups;
using Atlas.Party.Application.Queries.Lookups.LookupClassificationTypes;

namespace Atlas.Party.BffApi.Endpoints.Lookups.LookupClassificationTypes;

public sealed record LookupClassificationTypesResponse(string Code, string Name)
{
    public static IReadOnlyList<LookupClassificationTypesResponse> FromList(
        IReadOnlyList<ClassificationTypeLookupDto> result,
        IPartyLookupLabelLocalizer localizer
    )
    {
        return result
            .Select(x => new LookupClassificationTypesResponse(
                Code: x.Code,
                Name: localizer.GetClassificationTypeName(x.Code)
            ))
            .OrderBy(x => x.Name)
            .ToList();
    }
}
