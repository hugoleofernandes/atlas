using Atlas.BuildingBlocks.Permissions;
using Atlas.Platform.Resources.Permissions;
using Microsoft.Extensions.Localization;

namespace Atlas.Platform.Infrastructure.Labels;

public sealed class PlatformPermissionLabelProvider(IStringLocalizer<PlatformPermissionLabels> localizer)
    : IPermissionLabelProvider
{
    public string? Localize(string permissionCode)
    {
        var result = localizer[permissionCode];
        return result.ResourceNotFound ? null : result.Value;
    }
}
