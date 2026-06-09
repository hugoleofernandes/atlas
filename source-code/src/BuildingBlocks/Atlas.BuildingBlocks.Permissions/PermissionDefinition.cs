namespace Atlas.BuildingBlocks.Permissions;

using Atlas.SharedKernel.Modules;

/// <summary>
/// Canonical metadata describing a published permission code.
/// </summary>
public sealed record PermissionDefinition
{
    public PermissionDefinition(string code, bool isManager, AtlasModule module)
    {
        var parts = code.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            throw new InvalidOperationException($"Permission code '{code}' must follow '<module>.<group>.<action>' format.");

        ModuleId = module.Id;
        ModuleName = module.Name;
        Code = code;
        Group = parts[1];
        IsManager = isManager;
    }

    public Guid ModuleId { get; }
    public string ModuleName { get; }
    public string Code { get; }
    public string Group { get; }
    public bool IsManager { get; }

    public override string ToString() => Code;

    public static implicit operator string(PermissionDefinition definition) => definition.Code;
}
