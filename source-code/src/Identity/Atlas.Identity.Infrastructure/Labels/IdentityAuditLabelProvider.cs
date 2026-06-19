using Atlas.BuildingBlocks.Audit.Labels;
using Atlas.Identity.Contracts.EntityTypes;

namespace Atlas.Identity.Infrastructure.Labels;

/// <summary>
/// Provides audit labels for Identity module entity types.
/// Action localization is handled by a shared provider.
/// </summary>
public sealed class IdentityAuditLabelProvider : IAuditLabelProvider
{
    public string? LocalizeAction(string action) => null;

    public string? LocalizeEntityType(Guid entityTypeId)
    {
        if (entityTypeId == IdentityModuleEntityTypes.Users.EntityType.Id)
            return "User";
        if (entityTypeId == IdentityModuleEntityTypes.Roles.EntityType.Id)
            return "Role";
        if (entityTypeId == IdentityModuleEntityTypes.Invitations.EntityType.Id)
            return "Invitation";
        if (entityTypeId == IdentityModuleEntityTypes.Permissions.EntityType.Id)
            return "Permission";
        return null;
    }
}
