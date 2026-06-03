using Atlas.BuildingBlocks.AuditTrail.Labels;
using Microsoft.Extensions.Localization;
using StaffContracts = Atlas.Staff.Contracts;
using PlatformContracts = Atlas.Platform.Contracts;

namespace Atlas.SharedDomain.Resources.Audit;

/// <summary>
/// Provides localized audit labels for shared domain concepts.
/// Entity type localization is delegated to each module's own provider.
/// TODO: migrate LocalizeAction to Atlas.BuildingBlocks.AuditTrail.
/// </summary>
public sealed class SharedDomainAuditLabelProvider(IStringLocalizer<AuditLabels> localizer) : IAuditLabelProvider
{
    public string? LocalizeAction(string action) => Localize($"audit.action.{action}");

    public string? LocalizeEntityType(Guid entityTypeId)
    {
        var key =
            entityTypeId == StaffContracts.EntityTypes.StaffMemberId    ? "audit.entity.staff.staff-member"
            : entityTypeId == PlatformContracts.EntityTypes.TenantId    ? "audit.entity.platform.tenant"
            : null;

        return key is null ? null : Localize(key);
    }

    private string? Localize(string key)
    {
        var result = localizer[key];
        return result.ResourceNotFound ? null : result.Value;
    }
}
