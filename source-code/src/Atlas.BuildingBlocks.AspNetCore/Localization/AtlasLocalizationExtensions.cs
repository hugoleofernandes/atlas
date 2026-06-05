using Microsoft.Extensions.DependencyInjection;

namespace Atlas.BuildingBlocks.AspNetCore.Localization;

public static class AtlasLocalizationExtensions
{
    /// <summary>
    /// Registers Atlas localization using typed resource markers, intentionally without
    /// ResourcesPath.
    ///
    /// Example:
    ///   IStringLocalizer&lt;IdentityPermissionLabels&gt;
    ///
    /// resolves resources by the marker type namespace:
    ///   Atlas.Identity.Resources.Permissions.IdentityPermissionLabels
    ///
    /// expected files:
    ///   Atlas.Identity.Resources/Permissions/IdentityPermissionLabels.resx
    ///   Atlas.Identity.Resources/Permissions/IdentityPermissionLabels.pt.resx
    ///
    /// Keep resource file folders aligned with their marker class namespaces.
    /// Do not add ResourcesPath here unless every resource project is moved to the
    /// same "/Resources" folder convention.
    /// </summary>
    public static IServiceCollection AddAtlasLocalization(this IServiceCollection services) =>
        services.AddLocalization();
}
