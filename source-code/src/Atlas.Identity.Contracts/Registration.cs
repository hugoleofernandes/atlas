using Atlas.SharedKernel.Application.Seeding;
using ET = Atlas.Identity.Contracts.EntityTypes;

namespace Atlas.Identity.Contracts;

public sealed class Registration : IModuleRegistration
{
    public Guid ModuleId => Module.Id;
    public string ModuleName => Module.Name;

    public IReadOnlyList<EntityTypeRegistration> EntityTypes =>
    [
        new(ET.UserId,       "User",       "atlas_identity"),
        new(ET.RoleId,       "Role",       "atlas_identity"),
        new(ET.InvitationId, "Invitation", "atlas_identity"),
    ];
}
