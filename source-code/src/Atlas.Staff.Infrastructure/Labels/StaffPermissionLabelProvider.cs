using Atlas.BuildingBlocks.Permissions;
using Atlas.Staff.Resources.Permissions;
using Microsoft.Extensions.Localization;

namespace Atlas.Staff.Infrastructure.Labels;

public sealed class StaffPermissionLabelProvider(IStringLocalizer<StaffPermissionLabels> localizer)
    : IPermissionLabelProvider
{
    public string? Localize(string permissionCode)
    {
        var result = localizer[permissionCode];
        return result.ResourceNotFound ? null : result.Value;
    }
}
