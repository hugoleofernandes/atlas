using Atlas.SharedKernel.Modules;

namespace Atlas.Platform.Infrastructure.Seeders.Discovery;

internal interface IAtlasModuleDiscovery
{
    IReadOnlyList<AtlasModule> Discover();
}
