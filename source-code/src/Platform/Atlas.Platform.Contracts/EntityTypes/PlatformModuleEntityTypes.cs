using Atlas.SharedKernel.Modules;

namespace Atlas.Platform.Contracts.EntityTypes;

public sealed partial class PlatformModuleEntityTypes : IModuleEntityTypes
{
    public Guid ModuleId => AtlasModules.Platform.Id;
    public string ModuleName => AtlasModules.Platform.Name;

    private static readonly IReadOnlyList<AtlasEntityType> AllDefinitions =
    [
        Tenants.EntityType,
    ];

    public IReadOnlyList<AtlasEntityType> Definitions => AllDefinitions;
}
