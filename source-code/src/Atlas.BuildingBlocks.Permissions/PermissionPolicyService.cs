using Atlas.SharedKernel.Application;

namespace Atlas.BuildingBlocks.Permissions;

/// <summary>
/// Aggregates the canonical permission catalog into a single policy.
/// </summary>
public sealed class PermissionPolicyService : IPermissionPolicy
{
    public PermissionPolicyService(IEnumerable<IModulePermissions> modules)
    {
        var moduleList = modules.ToList();
        EnsureUniqueModules(moduleList);

        All = new HashSet<string>(moduleList.SelectMany(m => m.Permissions));
        EnsureUniquePermissions(moduleList);

        AllIncludingSystem = new HashSet<string>(All) { SystemPermissions.Root };

        Modules = moduleList
            .Select(m => new ModulePermissionCatalog(
                m.ModuleId,
                m.ModuleName,
                new HashSet<string>(m.Permissions),
                m.Groups))
            .ToList()
            .AsReadOnly();

        Groups = moduleList
            .SelectMany(m => m.Groups)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlySet<string> All { get; }
    public IReadOnlySet<string> AllIncludingSystem { get; }
    public IReadOnlyList<PermissionGroup> Groups { get; }
    public IReadOnlyList<ModulePermissionCatalog> Modules { get; }

    private static void EnsureUniqueModules(IReadOnlyList<IModulePermissions> modules)
    {
        var duplicateIds = modules
            .GroupBy(m => m.ModuleId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateIds.Count > 0)
            throw new InvalidOperationException($"Duplicate permission module ids: {string.Join(", ", duplicateIds)}");

        var duplicateNames = modules
            .GroupBy(m => m.ModuleName, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateNames.Count > 0)
            throw new InvalidOperationException($"Duplicate permission module names: {string.Join(", ", duplicateNames)}");
    }

    private static void EnsureUniquePermissions(IReadOnlyList<IModulePermissions> modules)
    {
        var duplicateCodes = modules
            .SelectMany(m => m.Permissions.Select(code => new { m.ModuleName, Code = code }))
            .GroupBy(x => x.Code)
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key} ({string.Join(", ", g.Select(x => x.ModuleName))})")
            .ToList();

        if (duplicateCodes.Count > 0)
            throw new InvalidOperationException($"Duplicate permission codes across modules: {string.Join("; ", duplicateCodes)}");
    }
}
