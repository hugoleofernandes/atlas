namespace Atlas.SharedKernel.Modules;

/// <summary>
/// Atlas module identities used across the application.
/// </summary>
public static class AtlasModules
{
    public static readonly AtlasModule Identity = new(new("00000000-0000-0000-0000-000000000001"), "identity", 1);

    public static readonly AtlasModule Platform = new(new("00000000-0000-0000-0000-000000000002"), "platform", 2);

    public static readonly AtlasModule Staff = new(new("00000000-0000-0000-0000-000000000003"), "staff", 3);

    public static readonly AtlasModule Party = new(new("00000000-0000-0000-0000-000000000004"), "party", 4);
}

public readonly record struct AtlasModule(Guid Id, string Name, int Code);
