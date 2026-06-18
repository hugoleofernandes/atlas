using Atlas.Party.Application.Queries.Lookups;
using Atlas.Party.Application.Queries.Persons.ListPersons;

namespace Atlas.Party.BffApi.Endpoints.Persons.ListPersons;

public sealed record ListPersonsClassificationResponse(string Code, string Name)
{
    public static IReadOnlyList<ListPersonsClassificationResponse> FromList(
        IReadOnlyList<ListPersonsClassificationDto> result,
        IPartyLookupLabelLocalizer localizer
    )
    {
        return result
            .Select(x => new ListPersonsClassificationResponse(
                Code: x.Code,
                Name: localizer.GetClassificationTypeName(x.Code)
            ))
            .OrderBy(x => x.Name)
            .ToList();
    }
}
