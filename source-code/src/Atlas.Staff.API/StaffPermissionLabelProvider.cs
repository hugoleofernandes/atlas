using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Staff.Resources.StaffPermissions;
using Microsoft.Extensions.Localization;

namespace Atlas.Staff.API;

/// <summary>
/// Provides localized labels for Staff module permission codes (staff.*).
/// Reads from StaffPermissionLabels.resx / StaffPermissionLabels.pt.resx.
/// </summary>
public sealed class StaffPermissionLabelProvider(
    IStringLocalizer<StaffPermissionLabels> localizer
) : IPermissionLabelProvider
{
    public string? Localize(string permissionCode)
    {
        var result = localizer[permissionCode];
        return result.ResourceNotFound ? null : result.Value;
    }
}
