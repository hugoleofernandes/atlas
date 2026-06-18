using Atlas.Platform.Application.Queries.Lookups;
using Atlas.Platform.Resources.Lookups;
using Microsoft.Extensions.Localization;

namespace Atlas.Platform.Infrastructure.Labels;

public sealed class PlatformLookupLabelLocalizer(IStringLocalizer<PlatformLookupLabels> localizer)
    : IPlatformLookupLabelLocalizer
{
    public string GetStatusName(string code)
    {
        var value = localizer[$"platform.lookup.status.{code}"];
        return value.ResourceNotFound ? code : value.Value;
    }
}
