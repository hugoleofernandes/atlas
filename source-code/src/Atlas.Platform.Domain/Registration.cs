using Atlas.SharedKernel.Application.Seeding;
using Atlas.SharedKernel.Platform.Domain;

namespace Atlas.Platform.Domain;

public sealed class Registration : IModuleRegistration
{
    public Guid ModuleId => PlatformEntityTypes.ModuleId;
    public string ModuleName => PlatformEntityTypes.ModuleName;

    public IReadOnlyList<EntityTypeRegistration> EntityTypes =>
        [new(PlatformEntityTypes.RootTenantId, "Tenant", "atlas_platform")];
}
