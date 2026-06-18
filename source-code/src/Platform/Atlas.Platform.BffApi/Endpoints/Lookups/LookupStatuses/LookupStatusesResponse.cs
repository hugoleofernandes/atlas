using Atlas.Platform.Application.Queries.Lookups;
using Atlas.Platform.Application.Queries.Lookups.LookupStatuses;

namespace Atlas.Platform.BffApi.Endpoints.Lookups.LookupStatuses;

public sealed record LookupStatusesResponse(string Code, string Name)
{
    public static IReadOnlyList<LookupStatusesResponse> FromList(
        IReadOnlyList<StatusLookupDto> result,
        IPlatformLookupLabelLocalizer localizer
    )
    {
        return result
            .Select(x => new LookupStatusesResponse(
                Code: x.Code,
                Name: localizer.GetStatusName(x.Code)
            ))
            .OrderBy(x => x.Name)
            .ToList();
    }
}
