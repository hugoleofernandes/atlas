namespace Atlas.BuildingBlocks.Permissions;

/// <summary>
/// Composite localizer that delegates to all registered IPermissionLabelProvider
/// implementations. Each module registers its own provider; this class aggregates them.
/// Falls back to the raw permission code if no provider claims the key.
/// </summary>
public sealed class PermissionLabelLocalizer(IEnumerable<IPermissionLabelProvider> providers)
{
    public string Localize(string permissionCode)
        => providers
               .Select(p => p.Localize(permissionCode))
               .FirstOrDefault(label => label is not null)
           ?? permissionCode;
}
