using Atlas.BuildingBlocks.Permissions;

namespace Atlas.Platform.Infrastructure.Labels;

/// <summary>
/// Platform has only 2 audit permission codes.
/// Hardcoded EN labels â€” add Atlas.Platform.Resources when PT support is needed.
/// </summary>
public sealed class PlatformPermissionLabelProvider : IPermissionLabelProvider
{
    public string? Localize(string permissionCode) => permissionCode switch
    {
        "platform.audit.read"   => "View platform audit log",
        "platform.audit.manage" => "Manage platform audit log",
        _ => null,
    };
}
