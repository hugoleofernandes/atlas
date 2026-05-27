using Atlas.API.Resources;
using Microsoft.Extensions.Localization;

namespace Atlas.API.Errors;

/// <summary>
/// Resolves a localized display label for a permission code.
/// Keys match PermissionCatalog codes; resources live in PermissionLabels.resx / PermissionLabels.pt.resx.
/// Culture is determined automatically from the request's Accept-Language header.
/// Falls back to the raw code if no translation is found.
/// </summary>
public sealed class PermissionLabelLocalizer(IStringLocalizer<PermissionLabels> localizer)
{
    public string Localize(string permissionCode)
    {
        var result = localizer[permissionCode];
        return result.ResourceNotFound ? permissionCode : result.Value;
    }
}
