using Atlas.SharedKernel.Modules;

namespace Atlas.Party.Contracts.EntityTypes;

public sealed partial class PartyModuleEntityTypes : IModuleEntityTypes
{
    public Guid ModuleId => AtlasModules.Party.Id;
    public string ModuleName => AtlasModules.Party.Name;

    private static readonly IReadOnlyList<AtlasEntityType> AllDefinitions =
    [
        Persons.EntityType,
        Organizations.EntityType,
    ];

    public IReadOnlyList<AtlasEntityType> Definitions => AllDefinitions;
}
