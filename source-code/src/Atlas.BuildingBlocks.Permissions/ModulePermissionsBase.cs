using Atlas.SharedKernel.Modules;

namespace Atlas.BuildingBlocks.Permissions;

/// <summary>
/// Base implementation for module-owned permission catalogs.
/// </summary>
public abstract class ModulePermissionsBase : IModulePermissions
{
    protected abstract AtlasModule Module { get; }
    protected abstract IReadOnlyList<PermissionDefinition> DefinitionsCore { get; }

    public Guid ModuleId => Module.Id;
    public string ModuleName => Module.Name;

    public IReadOnlySet<string> Permissions =>
        new HashSet<string>(DefinitionsCore.Select(definition => definition.Code), StringComparer.Ordinal);

    public IReadOnlyList<PermissionGroup> Groups => BuildGroups(DefinitionsCore);

    public IReadOnlyList<PermissionDefinition> Definitions => DefinitionsCore;

    private static IReadOnlyList<PermissionGroup> BuildGroups(IReadOnlyList<PermissionDefinition> definitions)
    {
        return definitions
            .Where(definition => definition.IsManager)
            .Select(definition => new PermissionGroup(
                definition.Code,
                definitions
                    .Where(candidate => candidate.Group == definition.Group && !candidate.IsManager)
                    .Select(candidate => candidate.Code)
                    .ToList()))
            .Where(group => group.Granular.Count > 0)
            .ToList()
            .AsReadOnly();
    }
}
