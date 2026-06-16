using Atlas.BuildingBlocks.Permissions;
using Atlas.Party.Resources.Permissions;
using Microsoft.Extensions.Localization;

namespace Atlas.Party.Infrastructure.Labels;

public sealed class PartyPermissionLabelProvider(IStringLocalizer<PartyPermissionLabels> localizer)
    : IPermissionLabelProvider
{
    public string? Localize(string permissionCode)
    {
        var result = localizer[permissionCode];
        return result.ResourceNotFound ? null : result.Value;
    }
}
