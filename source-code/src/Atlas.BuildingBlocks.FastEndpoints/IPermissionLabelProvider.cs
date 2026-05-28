namespace Atlas.BuildingBlocks.FastEndpoints;

/// <summary>
/// Provides localized labels for permission codes owned by a specific module.
/// Each module registers one implementation via DI.
/// The <see cref="PermissionLabelLocalizer"/> composite delegates to all registered providers.
/// </summary>
public interface IPermissionLabelProvider
{
    /// <summary>
    /// Returns the localized label for the given permission code,
    /// or <c>null</c> if this provider does not own that code.
    /// </summary>
    string? Localize(string permissionCode);
}
