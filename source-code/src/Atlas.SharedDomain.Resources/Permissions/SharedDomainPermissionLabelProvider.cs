using Atlas.BuildingBlocks.FastEndpoints;
using Microsoft.Extensions.Localization;

namespace Atlas.SharedDomain.Resources.Permissions;

/// <summary>
/// Provides localized labels for the canonical shared permission catalog.
/// </summary>
public sealed class SharedDomainPermissionLabelProvider(
    IStringLocalizer<PermissionLabels> localizer
) : IPermissionLabelProvider
{
    public string? Localize(string permissionCode)
    {
        var result = localizer[permissionCode];
        return result.ResourceNotFound ? null : result.Value;
    }
}
