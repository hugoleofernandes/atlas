using Atlas.BuildingBlocks.Audit.Labels;
using Microsoft.Extensions.Localization;

namespace Atlas.BuildingBlocks.Audit.Resources;

/// <summary>
/// Provides localized labels for audit actions (Added/Modified/Deleted).
/// Registered once in Atlas.API â€” applies to all modules.
/// </summary>
public sealed class AuditActionLabelProvider(IStringLocalizer<AuditActionLabels> localizer)
    : IAuditLabelProvider
{
    public string? LocalizeAction(string action)
    {
        var result = localizer[$"audit.action.{action}"];
        return result.ResourceNotFound ? null : result.Value;
    }

    public string? LocalizeEntityType(Guid entityTypeId) => null;
}

/// <summary>Marker class for IStringLocalizer resolution.</summary>
public sealed class AuditActionLabels;
