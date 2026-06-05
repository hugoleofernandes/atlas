using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Resources.Permissions;
using Microsoft.Extensions.Localization;

namespace Atlas.Identity.Infrastructure.Labels;

public sealed class IdentityPermissionLabelProvider(IStringLocalizer<IdentityPermissionLabels> localizer)
    : IPermissionLabelProvider
{
    public string? Localize(string permissionCode)
    {
        var result = localizer[permissionCode];
        return result.ResourceNotFound ? null : result.Value;
    }
}
