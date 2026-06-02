namespace Atlas.SharedDomain.Modules;

/// <summary>
/// Canonical module identifiers shared across platform registry, permission catalog,
/// audit queries, and frontend lookups.
/// </summary>
public static class AtlasModules
{
    public static readonly Guid Identity = new("4348add3-e86a-4e29-8adb-581fd2ae0871");
    public static readonly Guid Staff    = new("e24bf9cf-84aa-4f55-884c-9784e2278d2e");
    public static readonly Guid Platform = new("b4d35314-b7b1-4a56-8bb6-fd1beedf5070");

    public const string IdentityName = "identity";
    public const string StaffName    = "staff";
    public const string PlatformName = "platform";
}
