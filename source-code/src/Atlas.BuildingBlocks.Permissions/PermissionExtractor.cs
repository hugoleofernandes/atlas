using System.Reflection;

namespace Atlas.BuildingBlocks.Permissions;

/// <summary>
/// Derives Permissions and Groups from a module's constant declarations via reflection.
/// Runs once at startup when the IPermissionPolicy singleton is constructed.
/// </summary>
public static class PermissionExtractor
{
    private static readonly BindingFlags Fields = BindingFlags.Public | BindingFlags.Static;

    public static IReadOnlySet<string> ExtractAll(Type type)
    {
        var result = new HashSet<string>();
        Traverse(type, result);
        return result;
    }

    public static IReadOnlyList<PermissionGroup> ExtractGroups(Type type)
    {
        var groups = new List<PermissionGroup>();
        BuildGroups(type, groups);
        return groups.AsReadOnly();
    }

    public static IReadOnlyList<PermissionDefinition> ExtractDefinitions(Type type, Guid moduleId, string moduleName)
    {
        return ExtractAll(type)
            .OrderBy(code => code, StringComparer.Ordinal)
            .Select(code => PermissionDefinition.Parse(moduleId, moduleName, code))
            .ToList()
            .AsReadOnly();
    }

    private static void Traverse(Type type, HashSet<string> codes)
    {
        foreach (var field in type.GetFields(Fields))
            if (field.IsLiteral && field.FieldType == typeof(string))
                codes.Add((string)field.GetValue(null)!);

        foreach (var nested in type.GetNestedTypes(BindingFlags.Public))
            Traverse(nested, codes);
    }

    private static void BuildGroups(Type type, List<PermissionGroup> groups)
    {
        var constants = type.GetFields(Fields)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .ToList();

        var manageField = constants.FirstOrDefault(f => f.Name == "Manage");
        if (manageField is not null)
        {
            var manage = (string)manageField.GetValue(null)!;
            var granular = constants
                .Where(f => f.Name != "Manage")
                .Select(f => (string)f.GetValue(null)!)
                .ToList();

            if (granular.Count > 0)
                groups.Add(new PermissionGroup(manage, granular));
        }

        foreach (var nested in type.GetNestedTypes(BindingFlags.Public))
            BuildGroups(nested, groups);
    }
}
