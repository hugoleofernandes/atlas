using Atlas.BuildingBlocks.Audit.Labels;
using Atlas.SharedKernel.EntityTypes;

namespace Atlas.Staff.Infrastructure.Labels;

/// <summary>
/// Provides audit labels for Staff module entity types.
/// Action localization is handled by AuditActionLabelProvider in BuildingBlocks.
/// </summary>
public sealed class StaffAuditLabelProvider : IAuditLabelProvider
{
    public string? LocalizeAction(string action) => null;

    public string? LocalizeEntityType(Guid entityTypeId)
    {
        if (entityTypeId == StaffEntityTypes.StaffMemberId)
            return "Staff member";
        return null;
    }
}
