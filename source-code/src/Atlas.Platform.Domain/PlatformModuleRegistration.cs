using Atlas.SharedKernel.Application.Seeding;
using Atlas.SharedKernel.EntityTypes;

namespace Atlas.Platform.Domain;

public sealed class PlatformModuleRegistration : IModuleRegistration
{
    public Guid ModuleId => PlatformEntityTypes.ModuleId;
    public string ModuleName => PlatformEntityTypes.ModuleName;

    public IReadOnlyList<EntityTypeRegistration> EntityTypes =>
        [new(PlatformEntityTypes.RootTenantId, "Tenant", "atlas_platform")];
}
