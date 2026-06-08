using Atlas.SharedKernel.Modules;

namespace Atlas.Identity.Contracts.EntityTypes;

public sealed partial class IdentityModuleEntityTypes : IModuleEntityTypes
{
    public Guid ModuleId => AtlasModules.Identity.Id;
    public string ModuleName => AtlasModules.Identity.Name;

    private static readonly IReadOnlyList<AtlasEntityType> AllDefinitions =
    [
        Users.EntityType,
        Roles.EntityType,
        Invitations.EntityType,
    ];

    public IReadOnlyList<AtlasEntityType> Definitions => AllDefinitions;
}
