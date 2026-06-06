using System.Reflection;
using Atlas.SharedKernel.Modules;

namespace Atlas.Platform.Infrastructure.Seeders.Discovery;

internal sealed class AtlasEntityTypeDiscovery : IAtlasEntityTypeDiscovery
{
    public IReadOnlyList<AtlasEntityType> Discover()
    {
        return typeof(AtlasModules).Assembly
            .GetTypes()
            .Where(type => type.IsClass && type.IsAbstract && type.IsSealed)
            .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(field => field.FieldType == typeof(AtlasEntityType))
            .Select(field => (AtlasEntityType)field.GetValue(null)!)
            .OrderBy(entityType => entityType.Module.Name, StringComparer.Ordinal)
            .ThenBy(entityType => entityType.Name, StringComparer.Ordinal)
            .ToList();
    }
}
