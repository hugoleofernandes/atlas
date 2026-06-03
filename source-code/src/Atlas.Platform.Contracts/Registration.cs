using Atlas.SharedKernel.Application.Seeding;
using ET = Atlas.Platform.Contracts.EntityTypes;

namespace Atlas.Platform.Contracts;

public sealed class Registration : IModuleRegistration
{
    public Guid ModuleId => Module.Id;
    public string ModuleName => Module.Name;

    public IReadOnlyList<EntityTypeRegistration> EntityTypes =>
    [
        new(ET.TenantId, "Tenant", "atlas_platform"),
    ];
}
