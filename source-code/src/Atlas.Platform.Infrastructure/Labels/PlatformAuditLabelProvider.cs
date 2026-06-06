using Atlas.BuildingBlocks.Audit.Labels;
using Atlas.SharedKernel.EntityTypes;

namespace Atlas.Platform.Infrastructure.Labels;

/// <summary>
/// Provides audit labels for Platform module entity types.
/// Action localization is handled by AuditActionLabelProvider in BuildingBlocks.
/// </summary>
public sealed class PlatformAuditLabelProvider : IAuditLabelProvider
{
    public string? LocalizeAction(string action) => null;

    public string? LocalizeEntityType(Guid entityTypeId)
    {
        if (entityTypeId == PlatformEntityTypes.Tenant.Id)
            return "Tenant";
        return null;
    }
}
