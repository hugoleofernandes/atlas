using Atlas.SharedKernel.Application.Seeding;
using Atlas.SharedKernel.EntityTypes;

namespace Atlas.Staff.Domain.ModulePermissions;

public sealed class StaffModuleRegistration : IModuleRegistration
{
    public Guid ModuleId => StaffEntityTypes.ModuleId;
    public string ModuleName => StaffEntityTypes.ModuleName;

    public IReadOnlyList<EntityTypeRegistration> EntityTypes =>
        [new(StaffEntityTypes.StaffMemberId, "StaffMember", "atlas_staff")];
}
