using Atlas.BuildingBlocks.Audit.Labels;
using Atlas.SharedKernel.EntityTypes;
using Atlas.SharedKernel.Modules;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        if (entityTypeId == IdentityEntityTypes.User.Id)
            return "User";
        if (entityTypeId == IdentityEntityTypes.Role.Id)
            return "Role";
        if (entityTypeId == IdentityEntityTypes.Invitation.Id)
            return "Invitation";
        return null;
    }
}
