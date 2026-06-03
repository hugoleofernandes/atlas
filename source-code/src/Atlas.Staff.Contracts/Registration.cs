using Atlas.SharedKernel.Application.Seeding;
using ET = Atlas.Staff.Contracts.EntityTypes;

namespace Atlas.Staff.Contracts;

public sealed class Registration : IModuleRegistration
{
    public Guid ModuleId => Module.Id;
    public string ModuleName => Module.Name;

    public IReadOnlyList<EntityTypeRegistration> EntityTypes =>
    [
        new(ET.StaffMemberId, "StaffMember", "atlas_staff"),
    ];
}
