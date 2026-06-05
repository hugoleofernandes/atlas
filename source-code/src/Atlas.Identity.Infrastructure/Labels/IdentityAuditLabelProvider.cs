using Atlas.BuildingBlocks.Audit.Labels;
using Atlas.Identity.Contracts;

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
        if (entityTypeId == EntityTypes.UserId)       return "User";
        if (entityTypeId == EntityTypes.RoleId)       return "Role";
        if (entityTypeId == EntityTypes.InvitationId) return "Invitation";
        return null;
    }
}
