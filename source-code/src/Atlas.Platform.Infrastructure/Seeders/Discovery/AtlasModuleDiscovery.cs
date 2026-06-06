using System.Reflection;
using Atlas.SharedKernel.Modules;

namespace Atlas.Platform.Infrastructure.Seeders.Discovery;

internal sealed class AtlasModuleDiscovery : IAtlasModuleDiscovery
{
    public IReadOnlyList<AtlasModule> Discover()
    {
        return typeof(AtlasModules)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(AtlasModule))
            .Select(field => (AtlasModule)field.GetValue(null)!)
            .OrderBy(module => module.Name, StringComparer.Ordinal)
            .ToList();
    }
}
