using Atlas.SharedKernel.Modules;

namespace Atlas.Staff.Contracts.EntityTypes;

public sealed partial class StaffModuleEntityTypes : IModuleEntityTypes
{
    public Guid ModuleId => AtlasModules.Staff.Id;
    public string ModuleName => AtlasModules.Staff.Name;

    private static readonly IReadOnlyList<AtlasEntityType> AllDefinitions =
    [
        StaffMembers.EntityType,
    ];

    public IReadOnlyList<AtlasEntityType> Definitions => AllDefinitions;
}
