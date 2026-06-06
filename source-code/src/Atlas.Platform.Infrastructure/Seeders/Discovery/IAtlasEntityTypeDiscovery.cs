using Atlas.SharedKernel.Modules;

namespace Atlas.Platform.Infrastructure.Seeders.Discovery;

internal interface IAtlasEntityTypeDiscovery
{
    IReadOnlyList<AtlasEntityType> Discover();
}
