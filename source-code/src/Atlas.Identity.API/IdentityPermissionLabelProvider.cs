using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.Identity.Resources.Tenants._Roles._Permissions;
using Microsoft.Extensions.Localization;

namespace Atlas.Identity.API;

/// <summary>
/// Provides localized labels for Identity module permission codes
/// (tenant.roles.* and tenant.invitations.*).
/// Reads from IdentityPermissionLabels.resx / IdentityPermissionLabels.pt.resx.
/// </summary>
public sealed class IdentityPermissionLabelProvider(
    IStringLocalizer<PermissionLabels> localizer
) : IPermissionLabelProvider
{
    public string? Localize(string permissionCode)
    {
        var result = localizer[permissionCode];
        return result.ResourceNotFound ? null : result.Value;
    }
}
