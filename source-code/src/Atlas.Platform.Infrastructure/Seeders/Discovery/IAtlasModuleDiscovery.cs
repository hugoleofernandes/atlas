using Atlas.SharedKernel.Modules;

namespace Atlas.Platform.Infrastructure.Seeders.Discovery;

public interface IAtlasModuleDiscovery
{
    IReadOnlyList<AtlasModule> Discover();
}
