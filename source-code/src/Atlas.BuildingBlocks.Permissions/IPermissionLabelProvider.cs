namespace Atlas.BuildingBlocks.Permissions;

/// <summary>
/// Provides localized labels for permission codes owned by a specific module.
/// Each module registers one implementation via DI.
/// The PermissionLabelLocalizer composite delegates to all registered providers.
/// </summary>
public interface IPermissionLabelProvider
{
    string? Localize(string permissionCode);
}
